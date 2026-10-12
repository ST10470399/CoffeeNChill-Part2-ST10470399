using System.Text.Json;
using CoffeeNChill.Functions.Models;
using CoffeeNChill.Functions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions;

public class OrderFunctions
{
    private readonly OrderQueueService _queueService;
    private readonly OrderTableService _tableService;
    private readonly ILogger<OrderFunctions> _logger;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public OrderFunctions(
        OrderQueueService queueService,
        OrderTableService tableService,
        ILogger<OrderFunctions> logger)
    {
        _queueService = queueService;
        _tableService = tableService;
        _logger = logger;
    }

    [Function("QueueOrder")]
    public async Task<IActionResult> QueueOrder(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "orders/queue")] HttpRequest request)
    {
        Order? order;

        try
        {
            order = await JsonSerializer.DeserializeAsync<Order>(
                request.Body, JsonOptions);
        }
        catch (JsonException)
        {
            return new BadRequestObjectResult("Invalid order JSON.");
        }

        if (order == null ||
            string.IsNullOrWhiteSpace(order.OrderId) ||
            string.IsNullOrWhiteSpace(order.CustomerName) ||
            order.SelectedItemSKUs == null ||
            order.SelectedItemSKUs.Count == 0 ||
            order.SelectedItemSKUs.Any(string.IsNullOrWhiteSpace) ||
            order.TotalPrice <= 0 ||
            double.IsNaN(order.TotalPrice) ||
            double.IsInfinity(order.TotalPrice))
        {
            return new BadRequestObjectResult(
                "Provide OrderId, CustomerName, at least one SelectedItemSKU, and a positive TotalPrice.");
        }

        if (order.OrderId.Length > 100 ||
            order.CustomerName.Length > 200 ||
            order.SelectedItemSKUs.Any(sku => sku.Length > 100))
        {
            return new BadRequestObjectResult(
                "One or more order fields are too long.");
        }

        if (order.OrderTimestamp == default)
        {
            order.OrderTimestamp = DateTimeOffset.UtcNow;
        }

        order.Status = "Received";

        try
        {
            await _queueService.EnqueueOrderAsync(order);

            _logger.LogInformation(
                "Order {OrderId} added to the processing queue.",
                order.OrderId);

            return new ObjectResult(new
            {
                message = "Order queued successfully.",
                orderId = order.OrderId,
                status = order.Status
            })
            {
                StatusCode = StatusCodes.Status202Accepted
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "Failed to queue order {OrderId}.", order.OrderId);

            return new ObjectResult("Unable to queue the order.")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    [Function("ProcessQueuedOrder")]
    public async Task ProcessQueuedOrder(
        [QueueTrigger(
            "order-processing-queue",
            Connection = "AzureWebJobsStorage")] string message)
    {
        try
        {
            var order = JsonSerializer.Deserialize<Order>(
                message, JsonOptions);

            if (order == null ||
                string.IsNullOrWhiteSpace(order.OrderId) ||
                string.IsNullOrWhiteSpace(order.CustomerName) ||
                order.SelectedItemSKUs == null ||
                order.SelectedItemSKUs.Count == 0 ||
                order.TotalPrice <= 0 ||
                double.IsNaN(order.TotalPrice) ||
                double.IsInfinity(order.TotalPrice))
            {
                throw new JsonException(
                    "Queued order contains invalid or missing fields.");
            }

            if (order.OrderTimestamp == default)
            {
                order.OrderTimestamp = DateTimeOffset.UtcNow;
            }

            order.Status = "Received";

            await _tableService.SaveOrderAsync(order);

            var orderDate = order.OrderTimestamp.UtcDateTime
                .ToString("yyyy-MM-dd");

            _logger.LogInformation(
                "Order {OrderId} status: Received",
                order.OrderId);

            await Task.Delay(2000);

            await _tableService.UpdateStatusAsync(
                order.OrderId, orderDate, "Preparing");

            _logger.LogInformation(
                "Order {OrderId} status: Preparing",
                order.OrderId);

            await Task.Delay(2000);

            await _tableService.UpdateStatusAsync(
                order.OrderId, orderDate, "Ready");

            _logger.LogInformation(
                "Order {OrderId} status: Ready",
                order.OrderId);

            await Task.Delay(2000);

            await _tableService.UpdateStatusAsync(
                order.OrderId, orderDate, "Collected");

            _logger.LogInformation(
                "Order {OrderId} status: Collected",
                order.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "Failed to process queued order message.");

            // Rethrow so the Functions runtime can retry the message.
            // Messages exceeding the retry limit go to the poison queue.
            throw;
        }
    }

    [Function("UpdateOrderStatus")]
    public async Task<IActionResult> UpdateOrderStatus(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "put",
            Route = "orders/{orderDate}/{orderId}/status")] HttpRequest request,
        string orderDate,
        string orderId)
    {
        StatusRequest? body;

        try
        {
            body = await JsonSerializer.DeserializeAsync<StatusRequest>(
                request.Body, JsonOptions);
        }
        catch (JsonException)
        {
            return new BadRequestObjectResult("Invalid JSON.");
        }

        var allowedStatuses = new[]
        {
            "Preparing", "Ready", "Collected"
        };

        if (body == null ||
            string.IsNullOrWhiteSpace(body.Status) ||
            !allowedStatuses.Contains(
                body.Status, StringComparer.OrdinalIgnoreCase))
        {
            return new BadRequestObjectResult(
                "Status must be Preparing, Ready, or Collected.");
        }

        try
        {
            var order = await _tableService.GetOrderAsync(
                orderId, orderDate);

            if (order == null)
            {
                return new NotFoundObjectResult("Order not found.");
            }

            var nextStatus = body.Status.Trim();

            var validTransition =
                (order.Status == "Received" && nextStatus == "Preparing") ||
                (order.Status == "Preparing" && nextStatus == "Ready") ||
                (order.Status == "Ready" && nextStatus == "Collected");

            if (!validTransition)
            {
                return new BadRequestObjectResult(
                    $"Cannot change status from {order.Status} to {nextStatus}.");
            }

            await _tableService.UpdateStatusAsync(
                orderId, orderDate, nextStatus);

            _logger.LogInformation(
                "Order {OrderId} manually updated to {Status}.",
                orderId, nextStatus);

            return new OkObjectResult(new
            {
                orderId,
                status = nextStatus
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "Failed to update order {OrderId}.", orderId);

            return new ObjectResult("Unable to update order status.")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    private sealed class StatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}

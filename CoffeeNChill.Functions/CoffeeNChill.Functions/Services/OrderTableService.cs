using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;

namespace CoffeeNChill.Functions.Services;

public class OrderTableService
{
    private readonly TableClient _tableClient;

    public OrderTableService(StorageService storageService)
    {
        _tableClient = storageService
            .GetTableServiceClient()
            .GetTableClient("Orders");
    }

    public async Task SaveOrderAsync(Order order)
    {
        var timestamp = order.OrderTimestamp == default
            ? DateTimeOffset.UtcNow
            : order.OrderTimestamp;

        order.PartitionKey = timestamp.UtcDateTime.ToString("yyyy-MM-dd");
        order.RowKey = order.OrderId;
        order.OrderTimestamp = timestamp;
        order.Status = "Received";

        await _tableClient.CreateIfNotExistsAsync();

        var entity = new TableEntity(order.PartitionKey, order.RowKey)
        {
            ["OrderId"] = order.OrderId,
            ["CustomerName"] = order.CustomerName,
            ["SelectedItemSKUsJson"] =
                JsonSerializer.Serialize(order.SelectedItemSKUs),
            ["TotalPrice"] = order.TotalPrice,
            ["OrderTimestamp"] = timestamp.UtcDateTime,
            ["Status"] = order.Status
        };

        await _tableClient.UpsertEntityAsync(
            entity, TableUpdateMode.Replace);
    }

    public async Task<Order?> GetOrderAsync(
        string orderId, string orderDate)
    {
        await _tableClient.CreateIfNotExistsAsync();

        var response =
            await _tableClient.GetEntityIfExistsAsync<TableEntity>(
                orderDate, orderId);

        if (!response.HasValue)
            return null;

        var entity = response.Value;

        var timestamp = entity["OrderTimestamp"] is DateTime dt
            ? new DateTimeOffset(DateTime.SpecifyKind(
                dt, DateTimeKind.Utc))
            : DateTimeOffset.UtcNow;

        var skuJson = entity.GetString("SelectedItemSKUsJson");

        return new Order
        {
            PartitionKey = entity.PartitionKey,
            RowKey = entity.RowKey,
            OrderId = entity.GetString("OrderId") ?? string.Empty,
            CustomerName =
                entity.GetString("CustomerName") ?? string.Empty,
            SelectedItemSKUs = string.IsNullOrWhiteSpace(skuJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(skuJson)
                    ?? new List<string>(),
            TotalPrice = Convert.ToDouble(entity["TotalPrice"]),
            OrderTimestamp = timestamp,
            Status = entity.GetString("Status") ?? "Received",
            Timestamp = entity.Timestamp,
            ETag = entity.ETag
        };
    }

    public async Task UpdateStatusAsync(
        string orderId, string orderDate, string status)
    {
        await _tableClient.CreateIfNotExistsAsync();

        var response =
            await _tableClient.GetEntityIfExistsAsync<TableEntity>(
                orderDate, orderId);

        if (!response.HasValue)
            throw new InvalidOperationException(
                $"Order {orderId} was not found.");

        var entity = response.Value;
        entity["Status"] = status;

        await _tableClient.UpdateEntityAsync(
            entity, entity.ETag, TableUpdateMode.Replace);
    }
}

using System.Text.Json;
using Azure.Storage.Queues;
using CoffeeNChill.Functions.Models;

namespace CoffeeNChill.Functions.Services;

public class OrderQueueService
{
    private readonly QueueClient _queueClient;

    public OrderQueueService()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("AzureWebJobsStorage")
            ?? "UseDevelopmentStorage=true";

        var options = new QueueClientOptions
        {
            MessageEncoding = QueueMessageEncoding.Base64
        };

        _queueClient = new QueueClient(
            connectionString,
            "order-processing-queue",
            options);
    }

    public async Task EnqueueOrderAsync(Order order)
    {
        await _queueClient.CreateIfNotExistsAsync();

        var json = JsonSerializer.Serialize(order);
        await _queueClient.SendMessageAsync(json);
    }
}

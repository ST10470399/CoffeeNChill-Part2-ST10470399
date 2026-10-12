
using Azure;
using Azure.Data.Tables;

namespace CoffeeNChill.Functions.Models;

public class Order : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public List<string> SelectedItemSKUs { get; set; } = new();
    public double TotalPrice { get; set; }
    public DateTimeOffset OrderTimestamp { get; set; }
    public string Status { get; set; } = "Received";
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}

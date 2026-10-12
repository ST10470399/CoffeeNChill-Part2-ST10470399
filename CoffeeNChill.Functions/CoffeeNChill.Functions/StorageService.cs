using Azure.Data.Tables;
using Azure.Storage.Files.Shares;

namespace CoffeeNChill.Functions;

public class StorageService
{
    private readonly string connectionString;

    public StorageService()
    {
        connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
            ?? "UseDevelopmentStorage=true";
    }

    public TableServiceClient GetTableServiceClient()
    {
        return new TableServiceClient(connectionString);
    }

    public ShareServiceClient GetFileShareServiceClient()
    {
        return new ShareServiceClient(connectionString);
    }
}
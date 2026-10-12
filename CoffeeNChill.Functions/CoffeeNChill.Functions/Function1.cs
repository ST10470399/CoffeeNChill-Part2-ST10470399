using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace CoffeeNChill.Functions;
// CODE ATTRIBUTION
// [1] Microsoft, "TableServiceClient Constructor," Microsoft Learn. [Online]
// Available: https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableserviceclient.-ctor?view=azure-dotnet.
// [Accessed: Sep. 13, 2026].
//
// [2] Microsoft, "TableServiceClient.GetTableClient Method," Microsoft Learn. [Online]
// Available: https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableserviceclient.gettableclient?view=azure-dotnet.
// [Accessed: Sep. 13, 2026].
//
// [3] Microsoft, "TableClient.CreateIfNotExists Method," Microsoft Learn. [Online]
// Available: https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableclient.createifnotexists?view=azure-dotnet.
// [Accessed: Sep. 13, 2026].

public class Function1
{
    [Function("Function1")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("AzureWebJobsStorage")
            ?? "UseDevelopmentStorage=true";

        var tableServiceClient = new TableServiceClient(connectionString); //[1]

        var tableClient = tableServiceClient.GetTableClient("MenuItems"); //[2]

        tableClient.CreateIfNotExists();//[3]

        return new OkObjectResult("Connected to Azurite Table Storage successfully!");
    }
}

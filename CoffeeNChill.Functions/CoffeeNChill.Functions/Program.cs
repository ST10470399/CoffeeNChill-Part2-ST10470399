using CoffeeNChill.Functions;
using CoffeeNChill.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// CODE ATTRIBUTION
// [4] Microsoft, "Guide for running C# Azure Functions in an isolated worker process," Microsoft Learn. [Online]
// Available: https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide.
// [Accessed: Sep. 13, 2026].

var builder = FunctionsApplication.CreateBuilder(args); //[4]

builder.ConfigureFunctionsWebApplication(); //[4]

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddSingleton<StorageService>()
    .AddSingleton<MenuTableService>()
    .AddSingleton<StaffDocumentService>();

builder.Build().Run();
using Azure;
using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;

namespace CoffeeNChill.Functions.Services;

public class MenuTableService
{
    private readonly TableClient _tableClient;

    public MenuTableService(StorageService storageService)
    {
        var tableServiceClient = storageService.GetTableServiceClient();

        _tableClient = tableServiceClient.GetTableClient("MenuItems");

        _tableClient.CreateIfNotExists();
    }

    public async Task AddMenuItemAsync(MenuItem menuItem)
    {
        await _tableClient.AddEntityAsync(menuItem);
    }

    public async Task<List<MenuItem>> GetAllMenuItemsAsync()
    {
        var items = new List<MenuItem>();

        await foreach (var item in _tableClient.QueryAsync<MenuItem>())
        {
            items.Add(item);
        }

        return items;
    }

    public async Task<List<MenuItem>> GetMenuItemsByCategoryAsync(string category)
    {
        var items = new List<MenuItem>();

        await foreach (var item in _tableClient.QueryAsync<MenuItem>(
            item => item.PartitionKey == category))
        {
            items.Add(item);
        }

        return items;
    }

    public async Task<MenuItem?> GetMenuItemAsync(string category, string id)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<MenuItem>(
                category,
                id);

            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task UpdateMenuItemAsync(MenuItem menuItem)
    {
        await _tableClient.UpdateEntityAsync(
            menuItem,
            menuItem.ETag,
            TableUpdateMode.Replace);
    }

    public async Task DeleteMenuItemAsync(string category, string id)
    {
        await _tableClient.DeleteEntityAsync(category, id);
    }
}
using System.Net;
using System.Text.Json;
using CoffeeNChill.Functions.Models;
using CoffeeNChill.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CoffeeNChill.Functions;

public class MenuFunctions
{
    private readonly MenuTableService _menuTableService;

    public MenuFunctions(MenuTableService menuTableService)
    {
        _menuTableService = menuTableService;
    }

    [Function("CreateMenuItem")]
    public async Task<HttpResponseData> CreateMenuItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "menu")]
        HttpRequestData req)
    {
        try
        {
            var menuItem = await JsonSerializer.DeserializeAsync<MenuItem>(
                req.Body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (menuItem == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid menu item data.");
                return badRequest;
            }

            if (string.IsNullOrWhiteSpace(menuItem.PartitionKey) ||
                string.IsNullOrWhiteSpace(menuItem.RowKey) ||
                string.IsNullOrWhiteSpace(menuItem.Name))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync(
                    "Category, item ID and name are required.");
                return badRequest;
            }

            await _menuTableService.AddMenuItemAsync(menuItem);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(menuItem);

            return response;
        }
        catch (Exception ex)
        {
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync(
                $"Error creating menu item: {ex.Message}");

            return response;
        }
    }

    [Function("GetAllMenuItems")]
    public async Task<HttpResponseData> GetAllMenuItems(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu")]
        HttpRequestData req)
    {
        try
        {
            var menuItems = await _menuTableService.GetAllMenuItemsAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(menuItems);

            return response;
        }
        catch (Exception ex)
        {
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync(
                $"Error retrieving menu items: {ex.Message}");

            return response;
        }
    }

[Function("GetMenuItemsByCategory")]
public async Task<HttpResponseData> GetMenuItemsByCategory(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu/category/{category}")]
    HttpRequestData req,
    string category)
{
    try
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Category is required.");
            return badRequest;
        }

        var menuItems =
            await _menuTableService.GetMenuItemsByCategoryAsync(category);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(menuItems);

        return response;
    }
    catch (Exception ex)
    {
        var response = req.CreateResponse(HttpStatusCode.InternalServerError);
        await response.WriteStringAsync(
            $"Error retrieving menu items by category: {ex.Message}");

        return response;
    }
 }

[Function("UpdateMenuItem")]
public async Task<HttpResponseData> UpdateMenuItem(
    [HttpTrigger(AuthorizationLevel.Anonymous, "put",
        Route = "menu/{category}/{id}")]
    HttpRequestData req,
    string category,
    string id)
{
    try
    {
        if (string.IsNullOrWhiteSpace(category) ||
            string.IsNullOrWhiteSpace(id))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync(
                "Category and item ID are required.");
            return badRequest;
        }

        var existingItem =
            await _menuTableService.GetMenuItemAsync(category, id);

        if (existingItem == null)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync("Menu item not found.");
            return notFound;
        }

        var updatedData = await JsonSerializer.DeserializeAsync<MenuItem>(
            req.Body,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (updatedData == null)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid menu item data.");
            return badRequest;
        }

        existingItem.Name = updatedData.Name;
        existingItem.Description = updatedData.Description;
        existingItem.Price = updatedData.Price;
        existingItem.IsAvailable = updatedData.IsAvailable;

        await _menuTableService.UpdateMenuItemAsync(existingItem);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(existingItem);

        return response;
    }
    catch (Exception ex)
    {
        var response = req.CreateResponse(
            HttpStatusCode.InternalServerError);

        await response.WriteStringAsync(
            $"Error updating menu item: {ex.Message}");

        return response;
    }
}
[Function("DeleteMenuItem")]
public async Task<HttpResponseData> DeleteMenuItem(
    [HttpTrigger(AuthorizationLevel.Anonymous, "delete",
        Route = "menu/{category}/{id}")]
    HttpRequestData req,
    string category,
    string id)
{
    try
    {
        if (string.IsNullOrWhiteSpace(category) ||
            string.IsNullOrWhiteSpace(id))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync(
                "Category and item ID are required.");
            return badRequest;
        }

        var existingItem =
            await _menuTableService.GetMenuItemAsync(category, id);

        if (existingItem == null)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync("Menu item not found.");
            return notFound;
        }

        await _menuTableService.DeleteMenuItemAsync(category, id);

        var response = req.CreateResponse(HttpStatusCode.NoContent);

        return response;
    }
    catch (Exception ex)
    {
        var response = req.CreateResponse(
            HttpStatusCode.InternalServerError);

        await response.WriteStringAsync(
            $"Error deleting menu item: {ex.Message}");

        return response;
    }
}
}
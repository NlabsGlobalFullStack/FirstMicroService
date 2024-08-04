using FirstMicroService.Orders.WebAPI.Context;
using FirstMicroService.Orders.WebAPI.DTOs;
using FirstMicroService.Orders.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace FirstMicroService.Orders.WebAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[AllowAnonymous]
public sealed class OrdersController(MongoDbContext context, HttpClient httpClient) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var items = context.GetCollection<Order>("Orders");

        var orders = await items.Find(item => true).ToListAsync(cancellationToken);

        var orderDtos = new List<OrderDto>();

        var products = new Result<List<ProductDto>>();

        var productsEndpoint = $"http://{configuration.GetSection("HttpRequest:Products").Value}/getall";

        var message = await httpClient.GetAsync(productsEndpoint);

        if (message.IsSuccessStatusCode)
        {
            products = await message.Content.ReadFromJsonAsync<Result<List<ProductDto>>>();
        }

        foreach (var order in orders)
        {
            var orderDto = new OrderDto
            {
                Id = order.Id,
                CreatAt = order.CreatAt,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                Price = order.Price,
                ProductName = products!.Data!.First(p => p.Id == order.ProductId).Name
            };

            orderDtos.Add(orderDto);
        }

        var orderList = new Result<List<OrderDto>>(orderDtos);
        return Ok(orderList);
    }

    [HttpPost]
    public async Task<IActionResult> Create(List<CreateOrderDto> request, CancellationToken cancellationToken = default)
    {
        var items = context.GetCollection<Order>("Orders");
        List<Order> orders = new();
        foreach (var item in request)
        {
            Order order = new()
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price,
                CreatAt = DateTime.Now,
            };

            orders.Add(order);
        }

        await items.InsertManyAsync(orders);

        var successMessages = new Result<string>("Order created successfully");

        return Ok(successMessages);
    }
}

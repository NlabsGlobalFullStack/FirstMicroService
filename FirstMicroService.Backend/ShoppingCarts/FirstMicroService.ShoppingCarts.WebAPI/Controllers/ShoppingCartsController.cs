using FirstMicroService.ShoppingCarts.WebAPI.Context;
using FirstMicroService.ShoppingCarts.WebAPI.DTOs;
using FirstMicroService.ShoppingCarts.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace FirstMicroService.ShoppingCarts.WebAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[AllowAnonymous]
public sealed class ShoppingCartsController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(HttpClient httpClient, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        List<ShoppingCart> shoppingCarts = await context.ShoppingCarts.ToListAsync(cancellationToken);

        string productsEnpoint = $"http://{configuration.GetSection("HttpRequest:Products").Value}/getall";
        var message = await httpClient.GetAsync(productsEnpoint);

        var products = new Result<List<ProductDto>>();

        if (message.IsSuccessStatusCode)
        {
            products = await message.Content.ReadFromJsonAsync<Result<List<ProductDto>>>();
        }

        List<ShoppingCartDto> response = shoppingCarts.Select(s => new ShoppingCartDto()
        {
            Id = s.Id,
            ProductId = s.ProductId,
            Quantity = s.Quantity,
            ProductName = products!.Data!.First(p => p.Id == s.ProductId).Name,
            ProductPrice = products.Data!.First(p => p.Id == s.ProductId).Price
        }).ToList();

        var shoppingCartLists = new Result<List<ShoppingCartDto>>(response);


        return Ok(shoppingCartLists);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateShoppingCartDto request, CancellationToken cancellationToken = default)
    {
        var shoppingCart = new ShoppingCart
        {
            ProductId = request.ProductId,
            Quantity = request.Quantity
        };

        await context.ShoppingCarts.AddAsync(shoppingCart, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var successMessages = new Result<string>("Product added to cart successfully");

        return Ok(successMessages);
    }

    [HttpGet]
    public async Task<IActionResult> CreateOrder(HttpClient httpClient, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var shoppingCarts = await context.ShoppingCarts.ToListAsync(cancellationToken);

        var productsEndpoint = $"http://{configuration.GetSection("HttpRequest:Products").Value}/getall";

        var message = await httpClient.GetAsync(productsEndpoint);

        var products = new Result<List<ProductDto>>();

        if (message.IsSuccessStatusCode)
        {
            products = await message.Content.ReadFromJsonAsync<Result<List<ProductDto>>>();
        }

        List<CreateOrderDto> response = shoppingCarts.Select(s => new CreateOrderDto
        {
            ProductId = s.ProductId,
            Quantity = s.Quantity,
            Price = products!.Data!.First(p => p.Id == s.ProductId).Price
        }).ToList();

        string ordersEnpoint = $"http://{configuration.GetSection("HttpRequest:Orders").Value}/create";

        string stringJson = JsonSerializer.Serialize(response);
        var content = new StringContent(stringJson, Encoding.UTF8, "application/json");

        var orderMessage = await httpClient.PostAsync(ordersEnpoint, content);

        if (orderMessage.IsSuccessStatusCode)
        {
            List<ChangeProductStockDto> changeProductStockDtos = shoppingCarts.Select(s => new ChangeProductStockDto(s.ProductId, s.Quantity)).ToList();

            productsEndpoint = $"http://{configuration.GetSection("HttpRequest:Products").Value}/change-product-stock";

            string prodctsStringJson = JsonSerializer.Serialize(changeProductStockDtos);
            var productsContent = new StringContent(prodctsStringJson, Encoding.UTF8, "application/json");

            await httpClient.PostAsync(productsEndpoint, productsContent);

            context.RemoveRange(shoppingCarts);
            await context.SaveChangesAsync(cancellationToken);

        }

        var successMessages = new Result<string>("Order created successfully");

        return Ok(successMessages);

    }
}

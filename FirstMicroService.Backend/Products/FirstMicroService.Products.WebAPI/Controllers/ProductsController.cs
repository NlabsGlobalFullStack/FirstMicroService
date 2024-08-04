using FirstMicroService.Products.WebAPI.Context;
using FirstMicroService.Products.WebAPI.DTOs;
using FirstMicroService.Products.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nlabs.Result;

namespace FirstMicroService.Products.WebAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[AllowAnonymous]

public sealed class ProductsController(ApplicationDbContext context) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var products = await context.Products.OrderBy(p => p.Name).ToListAsync(cancellationToken);

        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto request, CancellationToken cancellationToken = default)
    {
        var productIsExists = await context.Products.AnyAsync(p => p.Name == request.Name, cancellationToken);

        if (productIsExists)
        {
            var errorMessage = Result<string>.Failure("Product not found");
            return BadRequest(errorMessage);
        }

        var product = new Product()
        {
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock,
        };

        await context.Products.AddAsync(product, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var successMessage = Result<string>.Succeed("Product create action is successfull");

        return Ok(successMessage);
    }

    [HttpPost]
    public async Task<IActionResult> ChangeProductStock(List<ChangeProductStockDto> request, CancellationToken cancellationToken = default)
    {
        foreach (var item in request)
        {
            var product = await context.Products.FindAsync(item.ProductId, cancellationToken);
            if (product is not null)
            {
                product.Stock = item.Quantity;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

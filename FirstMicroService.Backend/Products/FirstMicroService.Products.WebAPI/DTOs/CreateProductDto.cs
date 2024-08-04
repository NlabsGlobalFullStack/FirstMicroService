namespace FirstMicroService.Products.WebAPI.DTOs;

public sealed record CreateProductDto(
    string Name,
    decimal Price,
    int Stock);

namespace FirstMicroService.Products.WebAPI.DTOs;

public sealed record ChangeProductStockDto(
    Guid ProductId,
    int Quantity
    );
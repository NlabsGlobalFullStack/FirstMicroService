namespace FirstMicroService.ShoppingCarts.WebAPI.DTOs;

public sealed record ChangeProductStockDto(
    Guid ProductId,
    int Quantity
    );
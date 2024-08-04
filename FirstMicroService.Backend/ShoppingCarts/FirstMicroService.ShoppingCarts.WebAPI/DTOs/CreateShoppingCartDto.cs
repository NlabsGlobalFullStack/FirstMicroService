namespace FirstMicroService.ShoppingCarts.WebAPI.DTOs;

public sealed record CreateShoppingCartDto(
    Guid ProductId,
    int Quantity);

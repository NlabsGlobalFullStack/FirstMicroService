namespace FirstMicroService.Orders.WebAPI.DTOs;

public sealed record CreateOrderDto(
    Guid ProductId,
    int Quantity,
    decimal Price);

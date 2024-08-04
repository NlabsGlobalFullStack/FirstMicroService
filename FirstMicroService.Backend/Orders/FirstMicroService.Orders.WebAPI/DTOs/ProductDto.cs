namespace FirstMicroService.Orders.WebAPI.DTOs;

public sealed record ProductDto(
    Guid Id,
    string Name);
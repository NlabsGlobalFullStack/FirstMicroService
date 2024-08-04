namespace FirstMicroService.ShoppingCarts.WebAPI.DTOs;

public sealed record CreateOrderDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
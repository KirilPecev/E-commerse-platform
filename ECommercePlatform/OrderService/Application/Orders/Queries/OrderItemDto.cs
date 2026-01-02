namespace OrderService.Application.Orders.Queries
{
    public record OrderItemDto(
        Guid Id,
        Guid ProductVariantId,
        string ProductName,
        decimal UnitPrice,
        string Currency,
        int Quantity,
        decimal TotalPrice);
}

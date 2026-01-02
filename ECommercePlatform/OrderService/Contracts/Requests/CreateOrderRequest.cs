namespace OrderService.Contracts.Requests
{
    public record CreateOrderRequest(
        Guid CustomerId,
        Guid ProductVariantId,
        string ProductName,
        decimal Price,
        string Currency,
        int Quantity
        );
}

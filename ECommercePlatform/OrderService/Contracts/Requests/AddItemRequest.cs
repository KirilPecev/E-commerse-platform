namespace OrderService.Contracts.Requests
{
    public record AddItemRequest(
        Guid ProductVariantId,
        string ProductName,
        decimal Price,
        string Currency,
        int Quantity
        );
}
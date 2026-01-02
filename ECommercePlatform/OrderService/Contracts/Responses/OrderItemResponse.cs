namespace OrderService.Contracts.Responses
{
    public record OrderItemResponse(
        Guid Id,
        Guid ProductVariantId,
        string ProductName,
        decimal UnitPrice,
        string Currency,
        int Quantity,
        decimal TotalPrice);
}

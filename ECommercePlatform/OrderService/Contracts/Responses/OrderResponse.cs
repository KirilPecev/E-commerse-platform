using OrderService.Domain.Aggregates;
using OrderService.Domain.ValueObjects;

namespace OrderService.Contracts.Responses
{
    public record OrderResponse(
        Guid Id,
        Guid CustomerId,
        DateTime CreatedAt,
        OrderStatus Status,
        decimal TotalPrice,
        Address ShippingAddress,
        string? CancellationReason,
        DateTime? ShippedAt,
        string? TrackingNumber,
        List<OrderItemResponse> Items
        );
}

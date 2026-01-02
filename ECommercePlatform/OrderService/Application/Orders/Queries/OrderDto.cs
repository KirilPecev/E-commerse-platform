using OrderService.Domain.Aggregates;
using OrderService.Domain.ValueObjects;

namespace OrderService.Application.Orders.Queries
{
    public record OrderDto(
        Guid Id,
        Guid CustomerId,
        DateTime CreatedAt,
        OrderStatus Status,
        decimal TotalPrice,
        Address ShippingAddress,
        string? CancellationReason,
        DateTime? ShippedAt,
        string? TrackingNumber,
        List<OrderItemDto> Items
        );
}

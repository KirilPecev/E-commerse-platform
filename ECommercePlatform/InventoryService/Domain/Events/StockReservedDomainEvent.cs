
using ECommercePlatform.Domain.Events;

using MediatR;

namespace InventoryService.Domain.Events
{
    public class StockReservedDomainEvent : IDomainEvent, INotification
    {
        public Guid OrderId { get; }
        public Guid ProductId { get; }
        public int Quantity { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public StockReservedDomainEvent(
            Guid orderId,
            Guid productId,
            int quantity)
        {
            OrderId = orderId;
            ProductId = productId;
            Quantity = quantity;
        }
    }
}

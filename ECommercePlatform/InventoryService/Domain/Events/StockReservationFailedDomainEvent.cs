
using ECommercePlatform.Domain.Events;

using MediatR;

namespace InventoryService.Domain.Events
{
    public class StockReservationFailedDomainEvent : IDomainEvent, INotification
    {
        public Guid OrderId { get; }
        public Guid ProductId { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public StockReservationFailedDomainEvent(
            Guid orderId,
            Guid productId)
        {
            OrderId = orderId;
            ProductId = productId;
        }
    }
}

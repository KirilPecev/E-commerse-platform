
using ECommercePlatform.Domain.Events;

using MediatR;

namespace InventoryService.Domain.Events
{
    public class StockReleasedDomainEvent : IDomainEvent, INotification
    {
        public Guid OrderId { get; }
        public Guid ProductId { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public StockReleasedDomainEvent(
            Guid orderId,
            Guid productId)
        {
            OrderId = orderId;
            ProductId = productId;
        }
    }
}

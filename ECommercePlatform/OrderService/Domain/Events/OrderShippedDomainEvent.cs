
using ECommercePlatform.Domain.Events;

using MediatR;

namespace OrderService.Domain.Events
{
    public class OrderShippedDomainEvent : IDomainEvent, INotification
    {
        public Guid OrderId { get; }
        public string TrackingNumber { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public OrderShippedDomainEvent(Guid orderId, string trackingNumber)
        {
            OrderId = orderId;
            TrackingNumber = trackingNumber;
        }
    }
}

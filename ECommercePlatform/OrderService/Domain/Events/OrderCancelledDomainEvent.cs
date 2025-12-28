
using ECommercePlatform.Domain.Events;

using MediatR;

namespace OrderService.Domain.Events
{
    public class OrderCancelledDomainEvent : IDomainEvent, INotification
    {
        public Guid OrderId { get; }
        public string Reason { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public OrderCancelledDomainEvent(
            Guid orderId,
            string reason)
        {
            OrderId = orderId;
            Reason = reason;
        }
    }
}

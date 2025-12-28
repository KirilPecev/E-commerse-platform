
using ECommercePlatform.Domain.Events;

using MediatR;

namespace OrderService.Domain.Events
{
    public class OrderCreatedDomainEvent : IDomainEvent, INotification
    {
        public Guid OrderId { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public OrderCreatedDomainEvent(Guid orderId)
        {
            OrderId = orderId;
        }
    }
}

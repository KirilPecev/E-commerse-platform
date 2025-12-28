
using ECommercePlatform.Domain.Events;

using MediatR;

namespace OrderService.Domain.Events
{
    public class OrderPaidDomainEvent : IDomainEvent, INotification
    {
        public Guid OrderId { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public OrderPaidDomainEvent(Guid orderId)
        {
            OrderId = orderId;
        }
    }
}

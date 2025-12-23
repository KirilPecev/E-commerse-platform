
using MediatR;

namespace CatalogService.Domain.Events
{
    public class ProductUpdatedDomainEvent : IDomainEvent, INotification
    {
        public Guid ProductId { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public ProductUpdatedDomainEvent(Guid productId)
        {
            ProductId = productId;
        }
    }
}

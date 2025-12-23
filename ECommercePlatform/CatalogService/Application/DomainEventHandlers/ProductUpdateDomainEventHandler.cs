using CatalogService.Application.Interfaces;
using CatalogService.Domain.Events;

using ECommercePlatform.Events;

using MediatR;


namespace CatalogService.Application.DomainEventHandlers
{
    public class ProductUpdateDomainEventHandler
        (IEventPublisher eventPublisher) : INotificationHandler<ProductUpdatedDomainEvent>
    {
        public async Task Handle(ProductUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {
            await eventPublisher.PublishAsync(new ProductUpdatedIntegrationEvent
            {
                ProductId = notification.ProductId
            });
        }
    }
}

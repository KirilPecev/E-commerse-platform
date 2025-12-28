
using CatalogService.Application;

using ECommercePlatform.Domain.Events;

using MediatR;

namespace CatalogService.Infrastructure
{
    public class DomainEventDispatcher
        (IMediator mediator) : IDomainEventDispatcher
    {
        public async Task DispatchAsync(IDomainEvent domainEvent)
        {
            await mediator.Publish(domainEvent);
        }
    }
}

using ECommercePlatform.Domain.Events;

namespace ECommercePlatform.Domain.Abstractions
{
    public abstract class AggregateRoot : Entity
    {
        private readonly List<IDomainEvent> domainEvents = new();

        public IReadOnlyCollection<IDomainEvent> DomainEvents => domainEvents;

        protected void AddDomainEvent(IDomainEvent @event)
            => domainEvents.Add(@event);

        public void ClearDomainEvents()
            => domainEvents.Clear();
    }
}

using ECommercePlatform.Domain.Abstractions;

using OrderService.Domain.Events;
using OrderService.Domain.Exceptions;
using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Aggregates
{
    public class Order : AggregateRoot
    {
        public Guid CustomerId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public OrderStatus Status { get; private set; }
        public decimal TotalPrice { get; private set; }
        public Address ShippingAddress { get; private set; } = default!;
        public List<OrderItem> Items { get; private set; } = new();

        // Initialize non-nullable properties with default! to satisfy CS8618 for EF Core or serialization
        private Order()
        {
            Status = default!;
            Items = default!;
        }

        public Order(Guid customerId)
        {
            Id = Guid.NewGuid();
            CustomerId = customerId;
            CreatedAt = DateTime.UtcNow;
            Status = OrderStatus.Draft;

            this.AddDomainEvent(new OrderCreatedDomainEvent(Id));
        }

        public void AddItem(Guid productVariantId, string name, decimal price, int quantity)
        {
            if (Status != OrderStatus.Draft)
                throw new OrderDomainException("Cannot modify a finalized order.");

            if (quantity <= 0)
                throw new OrderDomainException("Quantity must be greater than zero.");

            OrderItem? existingOrderItem = Items.FirstOrDefault(i => i.ProductVariantId == productVariantId);
            if (existingOrderItem != default)
            {
                existingOrderItem.IncreaseQuantity(quantity);
            }
            else
            {
                Items.Add(new OrderItem(productVariantId, name, price, quantity));
            }

            RecalculateTotal();
        }

        public void MarkAsPaid()
        {
            if (Status != OrderStatus.Draft)
                throw new OrderDomainException("Order cannot be paid.");

            Status = OrderStatus.Paid;

            AddDomainEvent(new OrderPaidDomainEvent(Id));
        }

        public void SetShippingAddress(Address address)
        {
            if (Status != OrderStatus.Draft)
                throw new OrderDomainException("Cannot change address after payment.");

            ShippingAddress = address;
        }

        private void RecalculateTotal()
        {
            TotalPrice = this.Items.Sum(i => i.TotalPrice);
        }
    }
}

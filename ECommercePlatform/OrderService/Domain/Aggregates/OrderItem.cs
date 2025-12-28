using ECommercePlatform.Domain;

namespace OrderService.Domain.Aggregates
{
    public class OrderItem : Entity
    {
        public Guid ProductVariantId { get; private set; }
        public string ProductName { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal TotalPrice => UnitPrice * Quantity;

        // Initialize non-nullable properties with default! to satisfy CS8618 for EF Core or serialization
        private OrderItem()
        {
            ProductVariantId = default!;
            ProductName = default!;
        }

        public OrderItem(Guid productVariantId, string productName, decimal unitPrice, int quantity)
        {
            Id = Guid.NewGuid();
            ProductName = productName;
            ProductVariantId = productVariantId;
            UnitPrice = unitPrice;
            Quantity = quantity;
        }

        public void IncreaseQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

            Quantity += quantity;
        }
    }
}

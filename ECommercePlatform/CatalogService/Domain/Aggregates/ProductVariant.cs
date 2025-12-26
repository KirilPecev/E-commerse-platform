using CatalogService.Domain.Common;
using CatalogService.Domain.ValueObjects;

namespace CatalogService.Domain.Aggregates
{
    public class ProductVariant : Entity
    {
        public Product Product { get; private set; }
        public string Sku { get; private set; }
        public Money Price { get; private set; }
        public string? Size { get; private set; }
        public string? Color { get; private set; }
        public int StockQuantity { get; private set; }

        public ProductVariant(Product product, string sku, Money price, string? size, string? color, int stockQuantity)
        {
            Id = Guid.NewGuid();
            Product = product;
            Sku = sku;
            Price = price;
            Size = size;
            Color = color;
            StockQuantity = stockQuantity;
        }

        public void UpdateDetails(string sku, string? size, string? color, int stockQuantity)
        {
            if (stockQuantity < 0)
                throw new ArgumentException("Stock quantity cannot be negative", nameof(stockQuantity));

            Sku = sku;
            Size = size;
            Color = color;
            StockQuantity = stockQuantity;
        }

        public void ChangePrice(Money price)
        {
            Price = price;
        }
    }
}
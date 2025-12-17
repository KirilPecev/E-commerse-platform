using CatalogService.Domain.Events;
using CatalogService.Domain.Exceptions;
using CatalogService.Domain.ValueObjects;

namespace CatalogService.Domain.Aggregates
{
    public class Product : AggregateRoot
    {
        public ProductName Name { get; private set; }
        public Money Price { get; private set; }
        public Guid CategoryId { get; private set; }
        public ProductStatus Status { get; private set; }
        public string? Description { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public static Product Create(ProductName name, Money price, Guid categoryId, string? description = null)
        {
            Product product = new()
            {
                Id = Guid.NewGuid(),
                Name = name,
                Price = price,
                CategoryId = categoryId,
                Status = ProductStatus.Active,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            product.AddDomainEvent(new ProductCreatedDomainEvent(product.Id));

            return product;
        }

        public void AddProductVariant()
        {
            if (Status == ProductStatus.Inactive)
                throw new CatalogDomainException("Cannot add version to inactive product");
            AddDomainEvent(new ProductVersionAddedDomainEvent(Id));
        }

        public void ChangePrice(Money newPrice)
        {
            if (Status == ProductStatus.Inactive)
                throw new CatalogDomainException("Cannot change price of inactive product");

            Price = newPrice;
        }

        public void Deactivate()
        {
            if (Status == ProductStatus.Inactive) return;

            Status = ProductStatus.Inactive;
        }
    }
}

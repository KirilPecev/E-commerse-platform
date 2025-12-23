
using CatalogService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Products.Queries
{
    public class GetAllProductsQueryHandler
        (CatalogDbContext dbContext) : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
    {
        public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
            => await dbContext
                .Products
                .AsNoTracking()
                .Select(product => new ProductDto(
                    product.Id,
                    product.Name.Value,
                    product.Price.Amount,
                    product.Price.Currency
                ))
                .ToListAsync(cancellationToken);
    }
}

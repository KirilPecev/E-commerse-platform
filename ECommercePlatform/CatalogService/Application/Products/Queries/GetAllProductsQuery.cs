using MediatR;

namespace CatalogService.Application.Products.Queries
{
    public record GetAllProductsQuery : IRequest<IEnumerable<ProductDto>>;
}

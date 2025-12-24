using MediatR;

namespace CatalogService.Application.Products.Commands
{
    public record DeleteProductVariantCommand
        (Guid ProductId,
        Guid VariantId) : IRequest;
}
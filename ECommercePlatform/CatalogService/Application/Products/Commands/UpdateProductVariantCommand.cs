using MediatR;

namespace CatalogService.Application.Products.Commands
{
    public record UpdateProductVariantCommand
        (Guid ProductId,
         Guid VariantId,
         string Sku,
         decimal Amount,
         string Currency,
         int StockQuantity,
         string? Size,
         string? Color) : IRequest;
}
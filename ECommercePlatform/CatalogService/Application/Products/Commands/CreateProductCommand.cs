using MediatR;

namespace CatalogService.Application.Products.Commands
{
    public record CreateProductCommand(
        string Name,
        decimal Amount,
        string Currency,
        Guid CategoryId,
        string? Description) : IRequest<Guid>;
}

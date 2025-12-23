using MediatR;

namespace CatalogService.Application.Products.Commands
{
    public record UpdateProductCommand(
        Guid Id,
        string Name,
        decimal Amount,
        string Currency,
        Guid CategoryId,
        string? Description) : IRequest;
}
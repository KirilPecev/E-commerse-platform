using MediatR;

namespace CatalogService.Application.Products.Commands
{
    public record ActivateProductCommand(Guid Id) : IRequest;
}
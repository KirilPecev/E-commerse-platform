using MediatR;

namespace CatalogService.Application.Products.Commands
{
    public record DeactivateProductCommand(Guid Id) : IRequest;
}
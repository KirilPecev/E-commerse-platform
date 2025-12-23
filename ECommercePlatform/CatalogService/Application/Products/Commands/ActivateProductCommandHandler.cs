using CatalogService.Application.Exceptions;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Products.Commands
{
    public class ActivateProductCommandHandler
        (CatalogDbContext dbContext) : IRequestHandler<ActivateProductCommand>
    {
        public async Task Handle(ActivateProductCommand request, CancellationToken cancellationToken)
        {
            Product? product = await dbContext
                .Products
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product is null)
                throw new NotFoundException(nameof(Product), request.Id);

            product.Activate();

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

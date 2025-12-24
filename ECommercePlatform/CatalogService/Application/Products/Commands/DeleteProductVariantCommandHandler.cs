using CatalogService.Infrastructure.Persistence;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Products.Commands
{
    public class DeleteProductVariantCommandHandler
        (CatalogDbContext dbContext) : IRequestHandler<DeleteProductVariantCommand>
    {
        public async Task Handle(DeleteProductVariantCommand request, CancellationToken cancellationToken)
        {
            await dbContext
                .ProductVariants
                .Where(p => p.Id == request.VariantId
                         && p.ProductId == request.ProductId)
                .ExecuteDeleteAsync(cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

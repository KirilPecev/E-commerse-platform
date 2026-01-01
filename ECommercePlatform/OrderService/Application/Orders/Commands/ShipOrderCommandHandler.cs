
using MediatR;

using Microsoft.EntityFrameworkCore;

using OrderService.Domain.Aggregates;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Application.Orders.Commands
{
    public class ShipOrderCommandHandler
        (OrdersDbContext ordersDbContext) : IRequestHandler<ShipOrderCommand>
    {
        public async Task Handle(ShipOrderCommand request, CancellationToken cancellationToken)
        {
            Order? order = await ordersDbContext
                .Orders
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null) throw new KeyNotFoundException($"Order with ID {request.OrderId} not found.");

            order.MarkAsShipped(request.TrackingNumber);

            await ordersDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

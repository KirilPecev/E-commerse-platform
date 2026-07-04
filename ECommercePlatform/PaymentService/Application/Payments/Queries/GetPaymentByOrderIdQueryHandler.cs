using MediatR;

using Microsoft.EntityFrameworkCore;

using PaymentService.Application.Interfaces;

namespace PaymentService.Application.Payments.Queries
{
    public class GetPaymentByOrderIdQueryHandler
        (IPaymentDbContext paymentDbContext) : IRequestHandler<GetPaymentByOrderId, OrderPaymentDto?>
    {
        public async Task<OrderPaymentDto?> Handle(GetPaymentByOrderId request, CancellationToken cancellationToken)
            => await paymentDbContext
                .Payments
                .AsNoTracking()
                .Where(o => o.OrderId == request.OrderId)
                .Select(o => new OrderPaymentDto(
                    o.Id,
                    o.OrderId,
                    o.Amount,
                    o.Status,
                    o.PaymentMethod,
                    o.ProcessedAt)
                )
                .FirstOrDefaultAsync(cancellationToken);
    }
}

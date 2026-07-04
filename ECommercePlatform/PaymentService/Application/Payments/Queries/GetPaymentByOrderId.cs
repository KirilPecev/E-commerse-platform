using MediatR;

namespace PaymentService.Application.Payments.Queries
{
    public record GetPaymentByOrderId(Guid OrderId) : IRequest<OrderPaymentDto>;
}

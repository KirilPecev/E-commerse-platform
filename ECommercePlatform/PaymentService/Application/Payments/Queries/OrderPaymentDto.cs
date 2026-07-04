using PaymentService.Domain.Aggregates;
using PaymentService.Domain.ValueObjects;

namespace PaymentService.Application.Payments.Queries
{
    public record OrderPaymentDto(
        Guid Id,
        Guid OrderId,
        Money Amount,
        PaymentStatus Status,
        PaymentMethod PaymentMethod,
        DateTime ProcessedAt);
}

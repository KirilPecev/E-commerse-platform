namespace PaymentService.Contracts.Responses
{
    public record OrderPaymentResponse(
        Guid Id,
        Guid OrderId,
        decimal Amount,
        string Currency,
        string Status,
        string PaymentMethod,
        DateTime ProcessedAt);
}

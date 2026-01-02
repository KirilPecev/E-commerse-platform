namespace OrderService.Contracts.Requests
{
    public record ShipOrderRequest(
        string TrackingNumber
        );
}
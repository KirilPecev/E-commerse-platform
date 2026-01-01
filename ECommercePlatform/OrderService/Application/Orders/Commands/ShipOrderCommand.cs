using MediatR;

namespace OrderService.Application.Orders.Commands
{
    public record ShipOrderCommand(
        Guid OrderId,
        string TrackingNumber
        ) : IRequest;
}

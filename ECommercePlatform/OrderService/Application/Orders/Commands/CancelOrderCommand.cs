using MediatR;

namespace OrderService.Application.Orders.Commands
{
    public record CancelOrderCommand(
        Guid OrderId,
        string Reason
        ) : IRequest;
}

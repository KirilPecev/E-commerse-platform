using MediatR;

namespace OrderService.Application.Orders.Commands
{
    public record PayOrderCommand(
        Guid OrderId
        ) : IRequest;
}

using MediatR;

namespace OrderService.Application.Orders.Commands
{
    public record RemoveItemCommand(
        Guid OrderId,
        Guid ItemId) : IRequest;
}
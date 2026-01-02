using MediatR;

namespace OrderService.Application.Orders.Queries
{
    public record GetOrderByIdQuery(
        Guid OrderId
        ) : IRequest<OrderDto>;
}

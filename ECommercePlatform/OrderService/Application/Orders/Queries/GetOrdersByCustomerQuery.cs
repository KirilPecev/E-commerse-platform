using MediatR;

namespace OrderService.Application.Orders.Queries
{
    public record GetOrdersByCustomerQuery(
        Guid CustomerId
        ) : IRequest<List<OrderDto>>;
}

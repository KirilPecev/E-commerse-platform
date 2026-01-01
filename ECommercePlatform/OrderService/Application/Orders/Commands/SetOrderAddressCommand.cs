
using MediatR;

namespace OrderService.Application.Orders.Commands
{
    public record SetOrderAddressCommand(
        Guid OrderId,
        string Street,
        string City,
        string ZipCode,
        string Country
        ) : IRequest;
}

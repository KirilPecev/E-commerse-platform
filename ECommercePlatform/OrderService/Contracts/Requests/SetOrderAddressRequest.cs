namespace OrderService.Contracts.Requests
{
    public record SetOrderAddressRequest(
        string Street,
        string City,
        string State,
        string ZipCode,
        string Country
        );
}
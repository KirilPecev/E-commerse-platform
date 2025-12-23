namespace CatalogService.Contracts.Responses
{
    public record ProductResponse(
        Guid Id,
        string Name,
        decimal Price);
}

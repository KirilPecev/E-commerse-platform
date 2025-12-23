namespace CatalogService.Contracts.Requests
{
    public record CreateProductRequest(
        string Name,
        decimal Amount,
        string Currency,
        Guid CategoryId,
        string? Description);
}

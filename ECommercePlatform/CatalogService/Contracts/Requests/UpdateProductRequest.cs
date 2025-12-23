namespace CatalogService.Contracts.Requests
{
    public record UpdateProductRequest(
        Guid Id,
        string Name,
        decimal Amount,
        string Currency,
        Guid CategoryId,
        string? Description);
}

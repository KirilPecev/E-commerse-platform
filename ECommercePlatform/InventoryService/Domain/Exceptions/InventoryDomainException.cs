namespace InventoryService.Domain.Exceptions
{
    public class InventoryDomainException : Exception
    {
        public InventoryDomainException(string message) : base(message)
        {
        }
    }
}

using ECommercePlatform.Domain;

namespace InventoryService.Domain.Aggregates
{
    public class StockReservation : Entity
    {
        public Guid OrderId { get; private set; }
        public int Quantity { get; private set; }
        public DateTime ReservedAt { get; private set; }
        public ReservationStatus Status { get; private set; }

        private StockReservation() { }

        public StockReservation(Guid orderId, int quantity)
        {
            OrderId = orderId;
            Quantity = quantity;
            ReservedAt = DateTime.UtcNow;
            Status = ReservationStatus.Pending;
        }

        public void Confirm()
        {
            Status = ReservationStatus.Confirmed;
        }

        public void Release()
        {
            Status = ReservationStatus.Released;
        }
    }
}

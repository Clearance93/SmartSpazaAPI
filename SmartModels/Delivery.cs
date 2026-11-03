using SmartEnums;
using System.ComponentModel.DataAnnotations;

namespace SmartModels
{
    public class Delivery
    {
        [Key]
        public Guid DeliveryId { get; set; }

        public Guid DriverId { get; set; }

        public Guid SupplierId { get; set; }

        public Guid ShopId { get; set; }

        public string DeliveryNumber { get; set; } = NewDeliveryNumber();

        public DeliveryStatus DeliveryStatus { get; set; }

        public string? Priority { get; set; }

        public TimeOnly ScheduledTime { get; set; }

        public TimeOnly EstimatedArrival {  get; set; }

        public TimeOnly ActualArrival { get; set; }

        public string? Notes { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? Updated { get; set; }

        public static string NewDeliveryNumber()
        {
            return Guid.NewGuid().ToString("N")[..5].ToUpperInvariant();
        }
    }
}

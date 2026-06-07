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

        public string DeliveryNumber { get; set; } = Guid.NewGuid().ToString("N")
                                                                   .Substring(0, 5)
                                                                   .ToUpper();
        public DeliveryStatus DeliveryStatus { get; set; }

        public string? Priority { get; set; }

        public TimeOnly ScheduledTime { get; set; }

        public TimeOnly EstimatedArrival {  get; set; }

        public TimeOnly ActualArrival { get; set; }

        public string? Notes { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? Updated { get; set; }
    }
}

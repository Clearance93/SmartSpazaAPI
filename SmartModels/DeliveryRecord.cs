using System.ComponentModel.DataAnnotations;

namespace SmartModels
{
    public class DeliveryRecord
    {
        [Key]
        public Guid DeliveryRecordId { get; set; }

        public Guid DeliveryId { get; set; }

        public Guid DriverId { get; set; }

        public Guid ShopId { get; set; }

        public string? Ratings {  get; set; }

        public string? Comments { get; set; }
    }
}

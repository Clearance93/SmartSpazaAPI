using SmartEnums;

namespace SmartDto.Delivery
{
    public class DeliveryTrackingDto
    {
        public Guid DeliveryTrackingId { get; set; }

        public Guid DriverId { get; set; }

        public Guid? DeliveryId { get; set; }

        public DeliveryStatus DeliveryStatus { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public decimal? DistanceRemaining { get; set; }

        public TimeOnly EstimatedTime { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? Updated { get; set; }
    }
}

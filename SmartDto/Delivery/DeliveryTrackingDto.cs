using SmartEnums;

namespace SmartDto.Delivery
{
    public class DeliveryTrackingDto
    {
        public Guid DeliveryTrackingId { get; set; }

        public Guid DriverId { get; set; }

        public DeliveryStatus DeliveryStatus { get; set; }

        public string? Latitude { get; set; }

        public string? Longitude { get; set; }

        public string? DistanceRemaining { get; set; }

        public DateTime EstimatedTime { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? Updated { get; set; }
    }
}

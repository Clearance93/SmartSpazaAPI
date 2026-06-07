namespace SmartDto.Delivery
{
    public class DeliveryRecordDto
    {
        public Guid DeliveryRecordId { get; set; }

        public Guid DeliveryId { get; set; }

        public Guid DriverId { get; set; }

        public Guid ShopId { get; set; }

        public string? Ratings { get; set; }

        public string? Comments { get; set; }
    }
}

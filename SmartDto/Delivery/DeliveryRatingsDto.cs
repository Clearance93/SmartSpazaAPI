namespace SmartDto.Delivery
{
    public class DeliveryRatingsDto
    {
        public Guid DeliveryId { get; set; }

        public Guid DriverId { get; set; }

        public Guid ShopId { get; set; }

        public decimal? Ratings { get; set; }

        public string? Comments { get; set; }
    }
}

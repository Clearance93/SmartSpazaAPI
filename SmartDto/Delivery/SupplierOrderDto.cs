namespace SmartDto.Delivery
{
    /// <summary>
    /// One delivery from the supplier's point of view: the delivery, the shop it is
    /// going to, the driver, the item lines, the latest tracking point and the shop's
    /// rating. Assembled server-side so the supplier portal needs one call.
    /// </summary>
    public class SupplierOrderDto
    {
        public Guid DeliveryId { get; set; }

        public string DeliveryNumber { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? Priority { get; set; }

        public string? ScheduledTime { get; set; }

        public string? EstimatedArrival { get; set; }

        public string? ActualArrival { get; set; }

        public DateTime? CreationTime { get; set; }

        public string? Notes { get; set; }

        public Guid ShopId { get; set; }

        public string? ShopName { get; set; }

        public string? ShopOwnerName { get; set; }

        public string? ShopPhone { get; set; }

        public string? ShopEmail { get; set; }

        public Guid DriverId { get; set; }

        public string? DriverName { get; set; }

        public string? DriverPhone { get; set; }

        public decimal? Rating { get; set; }

        public string? RatingComment { get; set; }

        public List<ShopOrderItemDto> Items { get; set; } = new List<ShopOrderItemDto>();

        public ShopOrderTrackingDto? Tracking { get; set; }
    }
}

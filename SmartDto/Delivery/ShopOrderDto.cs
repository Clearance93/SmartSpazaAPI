namespace SmartDto.Delivery
{
    /// <summary>
    /// One purchase order as the shop owner sees it: the delivery, who supplies it,
    /// who is driving it, the item lines, the latest tracking point and their rating.
    /// Assembled server-side so the retailer portal needs a single call.
    /// </summary>
    public class ShopOrderDto
    {
        public Guid DeliveryId { get; set; }

        public string DeliveryNumber { get; set; } = string.Empty;

        /// <summary>Human-readable status name (Created, Accepted, InTransit, ...).</summary>
        public string Status { get; set; } = string.Empty;

        public string? Priority { get; set; }

        public string? ScheduledTime { get; set; }

        public string? EstimatedArrival { get; set; }

        public string? ActualArrival { get; set; }

        public DateTime? CreationTime { get; set; }

        public string? Notes { get; set; }

        public Guid SupplierId { get; set; }

        public string? SupplierName { get; set; }

        public string? SupplierPhone { get; set; }

        public Guid DriverId { get; set; }

        public string? DriverName { get; set; }

        public string? DriverPhone { get; set; }

        public decimal? Rating { get; set; }

        public string? RatingComment { get; set; }

        public List<ShopOrderItemDto> Items { get; set; } = new List<ShopOrderItemDto>();

        public ShopOrderTrackingDto? Tracking { get; set; }
    }

    public class ShopOrderItemDto
    {
        public string? ProductName { get; set; }

        public int Quantity { get; set; }

        public string? Unit { get; set; }

        public string? Weight { get; set; }
    }

    public class ShopOrderTrackingDto
    {
        public string Status { get; set; } = string.Empty;

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public decimal? DistanceRemaining { get; set; }

        public string? EstimatedTime { get; set; }

        public DateTime? Updated { get; set; }
    }

    /// <summary>A supplier that stocks the caller's catalogue, with their footprint there.</summary>
    public class ShopSupplierDto
    {
        public Guid SupplierId { get; set; }

        public string? SupplierName { get; set; }

        public string? ContactName { get; set; }

        public string? Email { get; set; }

        public string? ContactNumber { get; set; }

        public string? Address { get; set; }

        public string? LogoUrl { get; set; }

        public string? Ratings { get; set; }

        public string? TotalOrders { get; set; }

        public int ProductCount { get; set; }

        public decimal StockValue { get; set; }
    }
}

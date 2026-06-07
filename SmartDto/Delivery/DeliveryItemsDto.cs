namespace SmartDto.Delivery
{
    public class DeliveryItemsDto
    {
        public Guid DeliveryItemId { get; set; }

        public Guid DeliveryId { get; set; }

        public string? ProductName { get; set; }

        public string? StockKeepUnit { get; set; }

        public int Quantity { get; set; }

        public string? Unit { get; set; }

        public string? Weight { get; set; }
        public DateTime? CreationTime { get; set; }

        public DateTime? Updated { get; set; }
    }
}

namespace SmartDto.Inventory
{
    public class ProductDto
    {
        public Guid ProductId { get; set; }

        public string? Barcode { get; set; }

        public string? StockKeepUnit { get; set; }

        public string? Name { get; set; }

        public string? Category { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public string? Unit { get; set; }

        public Guid? SupplierId { get; set; }

        public Guid? ShopOwnerId { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? Updated { get; set; }
    }
}

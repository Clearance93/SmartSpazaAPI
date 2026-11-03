namespace SmartDto.Inventory
{
    public class CheckoutItemDto
    {
        public Guid ProductId { get; set; }

        public string? Barcode { get; set; }

        public int Quantity { get; set; }
    }
}

namespace SmartDto.Inventory
{
    public class CheckoutDto
    {
        public Guid? ShopOwnerId { get; set; }

        public List<CheckoutItemDto> Items { get; set; } = new List<CheckoutItemDto>();
    }
}

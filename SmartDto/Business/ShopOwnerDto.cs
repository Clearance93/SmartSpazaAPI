using SmartEnums;

namespace SmartDto.Business
{
    public class ShopOwnerDto
    {
        public Guid ShopOnwerId { get; set; }

        public string? NameOfTheShops { get; set; }

        public string? OwnerName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public string? Latitude { get; set; }

        public string? Longitude { get; set; }

        public string? Status { get; set; }

        public ShopType ShopType { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace SmartModels
{
    public class DeliveryRatings
    {
        [Key]
        public Guid DeliveryRatingId { get; set; }

        public Guid DeliveryId { get; set; }

        public Guid DriverId { get; set; }

        public Guid ShopId { get; set; }

        public decimal? Ratings {  get; set; }

        public string? Comments { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? Updated { get; set; }
    }
}

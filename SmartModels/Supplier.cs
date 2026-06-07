using System.ComponentModel.DataAnnotations;

namespace SmartModels
{
    public class Supplier
    {
        [Key]
        public Guid SupplierId { get; set; }

        public string? SupplierName { get; set; }

        public string? ContactName { get; set; }

        public string? Email { get; set; }

        public string? ContactNumber { get; set; } 

        public string? Address { get; set; }

        public string? LogoUrl { get; set; }

        public string? Ratings { get; set; }

        public string? TotalOrders { get; set; }

        public string? ActiveStatus { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

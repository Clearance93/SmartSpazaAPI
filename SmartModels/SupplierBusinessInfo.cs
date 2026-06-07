using System.ComponentModel.DataAnnotations;

namespace SmartModels
{
    public class SupplierBusinessInfo
    {
        [Key]
        public Guid SupplierBusinessInfoId {  get; set; }

        public string? BusinessName { get; set; }

        public string? TradingName { get; set; }

        public string? BusinessEmail { get; set; }

        public string? PhoneNumber { get; set; }

        public string? BusinessAddress { get; set; }

        public string? BusinessLogo { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? Updated { get; set; }
    }
}

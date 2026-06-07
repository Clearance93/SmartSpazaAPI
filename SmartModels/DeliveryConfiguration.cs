using System.ComponentModel.DataAnnotations;

namespace SmartModels
{
    public class DeliveryConfiguration
    {
        [Key]
        public Guid DeliveryConfigurationId { get; set; }

        public Guid SupplierBusinessInfoId { get; set; }

        public int DefualtDeliveryFee { get; set; }

        public string? EstimatedDeliveryWindow { get; set; }

        public string? ServiceArea {  get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? Updated { get; set; } 
    }
}

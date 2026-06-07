using System.ComponentModel.DataAnnotations;

namespace SmartModels
{
    public class DeliveryPreference
    {
        [Key]
        public Guid DeliveryPreferenceId { get; set; }

        public Guid BusinessProfileId { get; set; }

        public TimeOnly PreferedDeliveryTime { get; set; }

        public string? SpecialInstruction { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? Updated { get; set; }
    }
}

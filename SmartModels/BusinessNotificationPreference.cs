using System.ComponentModel.DataAnnotations;

namespace SmartModels
{
    public class BusinessNotificationPreference
    {
        [Key]
        public Guid BusinessNotificationPreferenceId { get; set; }

        public Guid BusinessProfileId { get; set; }

        public string? LowStockAlerts { get; set; }

        public string? DeliveryUpdates { get; set; }

        public string? OrderConfirmation {  get; set; }

        public string? PaymentReminder { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? Updated { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace SmartModels
{
    public class SupplierBusinessNotification
    {
        [Key]
        public Guid SupplierBusinessNotificationId {  get; set; }

        public Guid SupplierBusinessInfoId { get; set; }
        public SupplierBusinessInfo? SupplierBusinessInfo { get; set; }

        public string? NewOrder {  get; set; }

        public string? DeliverUpdate { get; set; }

        public string? LowStockAlerts { get; set; }

        public string? DriverIssue {  get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? Updated { get; set; }
    }
}

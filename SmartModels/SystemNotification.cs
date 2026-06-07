using System.ComponentModel.DataAnnotations;

namespace SmartModels
{
    public class SystemNotification
    {
        [Key]
        public Guid SystemNotificationId { get; set; }

        public Guid? Uid { get; set; }
        
        public bool NewAssignments { get; set; }

        public bool TrafficAlerts { get; set; }

        public bool CustomerMessages { get; set; }

        public bool SystemUpdate { get; set; }

        public bool SoundAlerts { get; set; }

        public bool Vibration {  get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? Updated { get; set; }
    }
}

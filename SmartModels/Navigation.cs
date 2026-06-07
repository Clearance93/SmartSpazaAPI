using System.ComponentModel.DataAnnotations;

namespace SmartModels
{
    public class Navigation
    {
        [Key]
        public Guid NavigationId { get; set; }

        public Guid DriverId { get; set; }
        public Drivers? Driver {  get; set; }

        public bool AutoReRoute { get; set; }

        public bool TrafficAlertsOnMap { get; set; }

        public bool VoiceGuidance {  get; set; }

        public bool OfflineMaps { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? Updated { get; set; }
    }
}

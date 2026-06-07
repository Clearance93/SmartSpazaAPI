using System.ComponentModel.DataAnnotations;

namespace SmartModels
{
    public class OperatingHours
    {
        [Key]
        public Guid OperatingHoursId { get; set; }

        public Guid BusinessProfileId { get; set; }
        public BusinessProfile? BusinessProfile { get; set; }

        public TimeOnly OpeningTime { get; set; }

        public TimeOnly CloseTime { get; set; }
    }
}

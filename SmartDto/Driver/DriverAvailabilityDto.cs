namespace SmartDto.Driver
{
    public class DriverAvailabilityDto
    {
        public Guid DriversAvailabilityId { get; set; }

        public Guid DriverId { get; set; }

        public string? DayOfTheWeek { get; set; }

        public bool IsAvailable { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? Updated { get; set; }
    }
}

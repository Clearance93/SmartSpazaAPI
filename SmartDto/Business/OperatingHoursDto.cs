namespace SmartDto.Business
{
    public class OperatingHoursDto
    {
        public Guid OperatingHoursId { get; set; }

        public Guid BusinessProfileId { get; set; }

        public TimeOnly OpeningTime { get; set; }

        public TimeOnly CloseTime { get; set; }
    }
}

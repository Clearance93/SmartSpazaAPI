namespace SmartDto.Driver
{
    public class DriversDto
    {
        public Guid DriverId { get; set; }

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumbers { get; set; }

        public string? WhatsAppNumber { get; set; }

        public string? DeliveryZone { get; set; }

        public string? ProfilePicture { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? Updated { get; set; }
    }
}

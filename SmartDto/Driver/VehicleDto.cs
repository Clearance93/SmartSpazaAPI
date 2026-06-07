namespace SmartDto.Driver
{
    public class VehicleDto
    {
        public Guid Vid { get; set; }

        public Guid DriverId { get; set; }

        public string? VehicleName { get; set; }

        public string? VehicleRegistration { get; set; }

        public string? LicenseNumber { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? Updated { get; set; }
    }
}

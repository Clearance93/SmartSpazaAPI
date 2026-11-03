namespace SmartDto
{
    public class UserDto
    {
        public string? Id { get; set; }

        public string? FullName { get; set; }

        public string? ProfilePicture { get; set; }

        public string? Email { get; set; }

        public string? Role { get; set; }

        public List<string>? Roles { get; set; }

        public string? WhatsAppNumber { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? Updated { get; set; }
    }
}

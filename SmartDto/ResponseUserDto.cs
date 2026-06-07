namespace SmartDto
{
    public class ResponseUserDto
    {
        public string? Token { get; set; }

        public DateTime Expiration { get; set; }

        public string? Email { get; set; }

        public List<string> Roles { get; set; } = new List<string>();

        public Guid? RoleUserId { get; set; }
    }
}

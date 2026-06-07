using Microsoft.AspNetCore.Identity;

namespace SmartModels
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }

        public string? ProfilePicture {  get; set; }

        public string? PasswordResetToken { get; set; }

        public string? Password {  get; set; }

        public string? WhatsAppNumber { get; set; }

        public DateTime? TokenExpiration { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? Updated { get; set; }

        //public List<string>? Roles { get; set; }
    }
}

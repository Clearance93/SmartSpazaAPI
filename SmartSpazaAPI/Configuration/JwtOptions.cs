using System.ComponentModel.DataAnnotations;

namespace SmartSpazaAPI.Configuration
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        public string? Key { get; set; }

        [Range(1, 1440)]
        public int AccessTokenMinutes { get; set; } = 15;

        [Range(1, 90)]
        public int RefreshTokenDays { get; set; } = 7;
    }
}

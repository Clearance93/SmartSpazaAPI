using System.ComponentModel.DataAnnotations;

namespace SmartModels
{
    public class RefreshToken
    {
        [Key]
        public Guid RefreshTokenId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string TokenHash { get; set; } = string.Empty;

        public DateTime Created { get; set; }

        public DateTime Expires { get; set; }

        public string? CreatedByIp { get; set; }

        public DateTime? Revoked { get; set; }

        public string? RevokedByIp { get; set; }

        public string? ReplacedByTokenHash { get; set; }

        public bool IsActive => Revoked == null && DateTime.UtcNow < Expires;
    }
}

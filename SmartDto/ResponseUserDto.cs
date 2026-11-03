namespace SmartDto
{
    public class ResponseUserDto
    {
        public string? Token { get; set; }

        public DateTime Expiration { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiration { get; set; }

        public string? Email { get; set; }

        public List<string> Roles { get; set; } = new List<string>();

        public Guid? RoleUserId { get; set; }

        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// False when the account was created but the confirmation email could not be sent.
        /// </summary>
        public bool ConfirmationEmailSent { get; set; } = true;
    }
}

using System.ComponentModel.DataAnnotations;

namespace SmartDto
{
    public class EmailRequestDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string? Email { get; set; }
    }
}

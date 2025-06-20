using System.ComponentModel.DataAnnotations;

namespace Project.Areas.Admin.Models.DTOs
{
    public class UpdatePasswordDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [MaxLength(20)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
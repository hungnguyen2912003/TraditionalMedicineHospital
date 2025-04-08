using Project.Models.Enums;

namespace Project.Areas.Staff.Models.DTOs
{
    public class PatientDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public GenderType Gender { get; set; }
        public string IdentityNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? EmailAddress { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}

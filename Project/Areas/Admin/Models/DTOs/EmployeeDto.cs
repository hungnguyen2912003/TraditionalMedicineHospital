using Project.Areas.Admin.Models.Enums.Employee;

namespace Project.Areas.Admin.Models.DTOs
{
    public class EmployeeDto
    {
        public string Code { get; set; }
        public string FullName { get; set; }
        public GenderType Gender { get; set; }
        public string IdentityCard { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DegreeType Degree { get; set; }
        public ProfessionalQualificationType ProfessionalQualification { get; set; }
        public string? Image { get; set; }
        public IFormFile? ImageFile { get; set; }
        public DateTime StartDate { get; set; }
        public EmployeeStatus Status { get; set; }
        public Guid EmployeeCategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }
    }
}

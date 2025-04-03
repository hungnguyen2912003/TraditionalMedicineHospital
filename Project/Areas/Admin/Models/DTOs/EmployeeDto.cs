using Project.Areas.Admin.Models.Enums.Employee;

namespace Project.Areas.Admin.Models.DTOs
{
    public class EmployeeDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public IFormFile? ImageFile { get; set; }
        public GenderType Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string IdentityNumber { get; set; }
        public string Address { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public DegreeType Degree { get; set; }
        public ProfessionalQualificationType ProfessionalQualification { get; set; }
        public DateTime StartDate { get; set; }
        public EmployeeStatus Status { get; set; }
        public Guid EmployeeCategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }
    }
}

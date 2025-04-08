using Project.Models.Enums;

namespace Project.Areas.Admin.Models.ViewModels
{
    public class EmployeeViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public GenderType Gender { get; set; }
        public string EmailAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime StartDate { get; set; }
        public EmployeeStatus Status { get; set; }
        public bool IsActive { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string DepName { get; set; } = string.Empty;
        public DegreeType Degree { get; set; }
        public ProfessionalQualificationType ProfessionalQualification { get; set; }
        public string Address { get; set; } = string.Empty;
    }
}

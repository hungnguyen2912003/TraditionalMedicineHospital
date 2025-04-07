using Project.Models.Enums;

namespace Project.Areas.Admin.Models.ViewModels
{
    public class EmployeeViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public GenderType Gender { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime StartDate { get; set; }
        public EmployeeStatus Status { get; set; }
        public bool IsActive { get; set; }
        public string CategoryName { get; set; }
        public string DepName { get; set; }
        public DegreeType Degree { get; set; }
        public ProfessionalQualificationType ProfessionalQualification { get; set; }
        public string Address { get; set; }
    }
}

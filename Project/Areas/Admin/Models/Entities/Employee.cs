using Project.Areas.Admin.Models.Commons;
using Project.Areas.Admin.Models.Enums.Employee;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("Employee")]
    public class Employee : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(10)]
        public string Code { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        public GenderType Gender { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        [StringLength(12, MinimumLength = 9)]
        public string IdentityNumber { get; set; }
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        public string? Image { get; set; }
        [Required]
        public DegreeType Degree { get; set; }
        [Required]
        public ProfessionalQualificationType ProfessionalQualification { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public EmployeeStatus Status { get; set; }
    }
}

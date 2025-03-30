using Project.Areas.Admin.Models.Commons;
using Project.Areas.Admin.Models.Enums.Employee;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    public class Employee : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(10)]
        public string? Code { get; set; }
        [Required]
        [StringLength(50)]
        public string? FullName { get; set; }
        [Required]
        public GenderType Gender { get; set; }
        [Required]
        [StringLength(12, MinimumLength = 9)]
        public string? IdentityCard { get; set; }
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        [Phone]
        public string? PhoneNumber { get; set; }
        [Required]
        [StringLength(500)]
        public string? Address { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        [StringLength(500)]
        public DegreeType Degree { get; set; }
        [Required]
        [StringLength(500)]
        public ProfessionalQualificationType ProfessionalQualification { get; set; }
        [Required]
        [StringLength(500)]
        public string? Image { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public EmployeeStatus Status { get; set; }


        // Foreign Key
        public Guid EmployeeCategoryId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        [ForeignKey("EmployeeCategoryId")]
        public virtual EmployeeCategory? EmployeeCategory { get; set; }
    }
}

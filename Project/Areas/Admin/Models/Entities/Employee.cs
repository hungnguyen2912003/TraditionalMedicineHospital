using Project.Areas.Staff.Models.Entities;
using Project.Models.Commons;
using Project.Models.Enums;
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
        public string Code { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        [Required]
        public GenderType Gender { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        [StringLength(12, MinimumLength = 9)]
        public string IdentityNumber { get; set; } = string.Empty;
        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Images { get; set; }
        [Required]
        public DegreeType Degree { get; set; }
        [Required]
        public ProfessionalQualificationType ProfessionalQualification { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public EmployeeStatus Status { get; set; }

        // Foreign Key
        public Guid EmployeeCategoryId { get; set; }
        public Guid DepartmentId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        ///
        [ForeignKey("EmployeeCategoryId")]
        public required virtual EmployeeCategory EmployeeCategory { get; set; }
        [ForeignKey("DepartmentId")]
        public required virtual Department Department { get; set; }
        public virtual User? User { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; } = new HashSet<Assignment>();
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new HashSet<Prescription>();
    }
}

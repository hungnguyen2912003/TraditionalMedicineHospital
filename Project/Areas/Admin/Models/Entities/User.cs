using Project.Areas.Staff.Models.Entities;
using Project.Models.Commons;
using Project.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("User")]
    public class User : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(20)]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        [Required]
        public RoleType Role { get; set; }
        public bool IsFirstLogin { get; set; }

        // Foreign Key
        public Guid? EmployeeId { get; set; }
        public Guid? PatientId { get; set; }

        // Relationship
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }
    }
}

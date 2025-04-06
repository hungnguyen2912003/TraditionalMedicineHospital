using Project.Areas.Admin.Models.Commons;
using Project.Areas.Admin.Models.Enums.Employee;
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
        [StringLength(10)]
        public string Code { get; set; }
        [Required]
        [StringLength(20)]
        public string Username { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        [Required]
        public RoleType Role { get; set; }

        // Foreign Key
        public Guid EmployeeId { get; set; }

        // Relationship
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }
    }
}

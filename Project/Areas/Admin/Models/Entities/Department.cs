using Project.Areas.Staff.Models.Entities;
using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("Department")]
    public class Department : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        /// 
        public virtual ICollection<TreatmentMethod> TreatmentMethods { get; set; } = new HashSet<TreatmentMethod>();
        public virtual ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();
    }
}

using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("EmployeeCategory")]
    public class EmployeeCategory : BaseEntity, ICodeEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(10)]
        public string? Code { get; set; }
        [Required]
        [StringLength(50)]
        public string? Name { get; set; }
        public string? Description { get; set; }

        /////////////////////////////////////////////////////
        /// Relationship
        public virtual ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();
    }
}

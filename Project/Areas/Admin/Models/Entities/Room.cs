using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project.Areas.Staff.Models.Entities;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("Room")]
    public class Room : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        [Required]
        [StringLength(20)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // ForeignKey
        public Guid DepartmentId { get; set; }
        public Guid TreatmentMethodId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        [ForeignKey("DepartmentId")]
        public required virtual Department Department { get; set; }
        [ForeignKey("TreatmentMethodId")]
        public required virtual TreatmentMethod TreatmentMethod { get; set; } 
        public virtual ICollection<TreatmentRecordDetail> TreatmentRecordDetails { get; set; } = new HashSet<TreatmentRecordDetail>();
        public virtual ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();
    }
}

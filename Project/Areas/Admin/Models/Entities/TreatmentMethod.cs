using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("TreatmentMethod")]
    public class TreatmentMethod : BaseEntity
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
        [Required]
        public decimal Cost { get; set; }

        // ForeignKey
        public Guid DepartmentId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        /// 
        [ForeignKey("DepartmentId")]
        public required virtual Department Department { get; set; }
        public virtual ICollection<Room> Rooms { get; set; } = new HashSet<Room>();
    }
}

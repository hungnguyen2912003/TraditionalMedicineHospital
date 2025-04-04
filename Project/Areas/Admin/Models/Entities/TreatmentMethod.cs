using Project.Areas.Admin.Models.Commons;
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
        public string Code { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public decimal Cost { get; set; }

        //// ForeignKey
        //public Guid DepartmentId { get; set; }

        ///////////////////////////////////////////////////////
        ///// Relationships
        //[ForeignKey("DepartmentId")]
        //public virtual Department? Department { get; set; }
    }
}

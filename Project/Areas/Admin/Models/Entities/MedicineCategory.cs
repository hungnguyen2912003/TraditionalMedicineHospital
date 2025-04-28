using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("MedicineCategory")]
    public class MedicineCategory : BaseEntity
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
        /// Relationship
        public virtual ICollection<Medicine> Medicines { get; set; } = new HashSet<Medicine>();
    }
}

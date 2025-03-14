using Project.Areas.Admin.Models.Commons;
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
        public string Code { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [StringLength(500)]
        public string? Description { get; set; }
        [Required]
        public string Image { get; set; }
    }
}

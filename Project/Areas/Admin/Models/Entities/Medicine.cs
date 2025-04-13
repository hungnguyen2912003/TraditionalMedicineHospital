using Project.Areas.Staff.Models.Entities;
using Project.Models.Commons;
using Project.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("Medicine")]
    public class Medicine : BaseEntity, ICodeEntity
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
        public string? Images { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int StockQuantity { get; set; }
        [Required]
        [StringLength(10)]
        public UnitType StockUnit { get; set; }
        [Required]
        public ManufacturerType Manufacturer { get; set; }
        [Required]
        public DateTime ManufacturedDate { get; set; }
        [Required]
        public string ActiveIngredient { get; set; } = string.Empty;
        [Required]
        public DateTime ExpiryDate { get; set; }
        public Guid MedicineCategoryId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationship
        [ForeignKey("MedicineCategoryId")]
        public required virtual MedicineCategory MedicineCategory { get; set; }
        public virtual ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new HashSet<PrescriptionDetail>();
    }
}

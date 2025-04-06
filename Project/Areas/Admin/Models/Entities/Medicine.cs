using Project.Areas.Admin.Models.Commons;
using Project.Areas.Admin.Models.Enums.Medicines;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("Medicine")]
    public class Medicine : BaseEntity
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
        public string? Images { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int StockQuantity { get; set; }
        [Required]
        [StringLength(10)]
        public UnitType StockUnit { get; set; }
        [Required]
        [StringLength(50)]
        public string Manufacturer { get; set; }
        [Required]
        public DateTime ManufacturedDate { get; set; }
        [Required]
        public string ActiveIngredient { get; set; }
        [Required]
        public DateTime ExpiryDate { get; set; }
        public Guid MedicineCategoryId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationship
        [ForeignKey("MedicineCategoryId")]
        public virtual MedicineCategory MedicineCategory { get; set; }
    }
}

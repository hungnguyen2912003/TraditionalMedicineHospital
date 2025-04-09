using Project.Areas.Admin.Models.Entities;
using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Staff.Models.Entities
{
    [Table("PrescriptionDetail")]
    public class PrescriptionDetail : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public string Note { get; set; } = string.Empty;
        //Foreign key
        public Guid PrescriptionId { get; set; }
        public Guid MedicineId { get; set; }
        /////////////////////////////////////////////////////
        /// Relationships
        ///
        [ForeignKey("PrescriptionId")]
        public required virtual Prescription Prescription { get; set; }
        [ForeignKey("MedicineId")]
        public required virtual Medicine Medicine { get; set; }
    }
}

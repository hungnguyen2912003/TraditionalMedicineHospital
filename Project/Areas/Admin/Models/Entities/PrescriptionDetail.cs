using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("PrescriptionDetail")]
    public class PrescriptionDetail : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        //Foreign key
        public Guid PrescriptionId { get; set; }
        public Guid MedicineId { get; set; }
        /////////////////////////////////////////////////////
        /// Relationships
        ///
        [ForeignKey("PrescriptionId")]
        public virtual Prescription? Prescription { get; set; }
        [ForeignKey("MedicineId")]
        public virtual Medicine? Medicine { get; set; }
    }
}

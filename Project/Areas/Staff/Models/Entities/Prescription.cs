using Project.Areas.Admin.Models.Entities;
using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Staff.Models.Entities
{
    [Table("Prescription")]
    public class Prescription : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(10)]
        public string Code { get; set; }
        public DateTime PrescriptionDate { get; set; }
        public decimal TotalCost { get; set; }
        public string? Note { get; set; }

        //Foreign key
        public Guid TreatmentRecordId { get; set; }
        public Guid EmployeeId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        ///
        [ForeignKey("TreatmentRecordId")]
        public required virtual TreatmentRecord TreatmentRecord { get; set; }
        [ForeignKey("EmployeeId")]
        public required virtual Employee Employee { get; set; }
        public virtual ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new HashSet<PrescriptionDetail>();
    }
}

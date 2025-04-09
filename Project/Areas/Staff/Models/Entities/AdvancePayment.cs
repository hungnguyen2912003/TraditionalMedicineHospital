using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Staff.Models.Entities
{
    [Table("AdvancePayment")]
    public class AdvancePayment : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public int PaymentMethod { get; set; }
        public string? Note { get; set; } = string.Empty;
        public int Status { get; set; }

        //Foreign Key
        public Guid TreatmentRecordId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        /// 
        [ForeignKey("TreatmentRecordId")]
        public required virtual TreatmentRecord TreatmentRecord { get; set; } = null!;
    }
}

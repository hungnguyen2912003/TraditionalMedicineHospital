using Project.Models.Commons;
using Project.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("AdvancePayment")]
    public class AdvancePayment : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public PaymentType? Type { get; set; }
        public Guid TreatmentRecordId { get; set; }
        /////////////////////////////////////////////////////
        /// Relationships
        ///
        [ForeignKey("TreatmentRecordId")]
        public required virtual TreatmentRecord TreatmentRecord { get; set; }
    }
}

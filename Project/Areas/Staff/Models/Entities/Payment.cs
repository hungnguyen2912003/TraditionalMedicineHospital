using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Staff.Models.Entities
{
    [Table("Payment")]
    public class Payment : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? Note { get; set; } = string.Empty;
        public int Status { get; set; }
        public decimal TotalPrescriptionCost { get; set; }
        public decimal TotalTreatmentMethodCost { get; set; }
        public decimal InsuranceAmount { get; set; }
        public decimal TotalCost { get; set; }

        //Foreign Key
        public Guid TreatmentRecordId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        ///
        [ForeignKey("TreatmentRecordId")]
        public required virtual TreatmentRecord TreatmentRecord { get; set; }
    }
}

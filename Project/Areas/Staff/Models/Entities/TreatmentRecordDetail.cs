using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Staff.Models.Entities
{
    [Table("TreatmentRecordDetail")]
    public class TreatmentRecordDetail : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public string? Note { get; set; }
        //Foreign key
        public Guid TreatmentRecordId { get; set; }
        public Guid TreatmentTrackingId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        ///
        [ForeignKey("TreatmentRecordId")]
        public virtual TreatmentRecord TreatmentRecord { get; set; } = null!;
        [ForeignKey("TreatmentTrackingId")]
        public virtual TreatmentTracking TreatmentTracking { get; set; } = null!;
    }
}

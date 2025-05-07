using Project.Areas.Admin.Models.Entities;
using Project.Models.Commons;
using Project.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Staff.Models.Entities
{
    [Table("TreatmentTracking")]
    public class TreatmentTracking : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        public DateTime TrackingDate { get; set; }
        public string? Note { get; set; }
        public TrackingStatus Status { get; set; }
        public Guid? TreatmentRecordDetailId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        ///
        [ForeignKey("TreatmentRecordDetailId")]
        public virtual TreatmentRecordDetail? TreatmentRecordDetail { get; set; }
    }
}

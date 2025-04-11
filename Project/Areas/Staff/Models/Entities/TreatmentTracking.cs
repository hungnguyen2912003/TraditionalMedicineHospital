using Project.Areas.Admin.Models.Entities;
using Project.Models.Commons;
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
        public DateTime TreatmentDate { get; set; }
        public string? Note { get; set; }

        public Guid TreatmentMethodId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        ///
        public virtual ICollection<TreatmentRecordDetail> TreatmentRecordDetails { get; set; } = new HashSet<TreatmentRecordDetail>();

        [ForeignKey("TreatmentMethodId")]
        public virtual TreatmentMethod TreatmentMethod { get; set; } = null!;
    }
}

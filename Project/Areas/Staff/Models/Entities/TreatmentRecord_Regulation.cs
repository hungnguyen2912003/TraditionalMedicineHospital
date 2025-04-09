using Project.Areas.Admin.Models.Entities;
using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Staff.Models.Entities
{
    [Table("TreatmentRecord_Regulation")]
    public class TreatmentRecord_Regulation : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime ExecutionDate { get; set; }
        public string? Note { get; set; }

        //Foreign Key
        public Guid TreatmentRecordId { get; set; }
        public Guid RegulationId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        ///

        [ForeignKey("TreatmentRecordId")]
        public virtual TreatmentRecord TreatmentRecord { get; set; } = null!;
        [ForeignKey("RegulationId")]
        public virtual Regulation Regulation { get; set; } = null!;
    }
}

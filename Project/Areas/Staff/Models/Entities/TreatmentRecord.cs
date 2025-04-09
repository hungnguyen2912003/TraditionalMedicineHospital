using Project.Models.Commons;
using Project.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Staff.Models.Entities
{
    [Table("TreatmentRecord")]
    public class TreatmentRecord : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Note { get; set; } = string.Empty;
        public TreatmentStatus Status { get; set; }

        //Foreign key
        public Guid PatientId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        ///
        [ForeignKey("PatientId")]
        public required virtual Patient Patient { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; } = new HashSet<Assignment>();
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new HashSet<Prescription>();
        public virtual ICollection<TreatmentRecord_Regulation> TreatmentRecord_Regulations { get; set; } = new HashSet<TreatmentRecord_Regulation>();
    }
}

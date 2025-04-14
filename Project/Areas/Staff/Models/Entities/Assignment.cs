using Project.Areas.Admin.Models.Entities;
using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Staff.Models.Entities
{
    [Table("Assignment")]
    public class Assignment : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Note { get; set; }

        //Foreign Key
        public Guid EmployeeId { get; set; }
        public Guid TreatmentRecordId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        ///

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;
        [ForeignKey("TreatmentRecordId")]
        public virtual TreatmentRecord TreatmentRecord { get; set; } = null!;
    }
}

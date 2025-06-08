using Project.Models.Commons;
using Project.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("WarningSent")]
    public class WarningSent : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public Guid TreatmentRecordDetailId { get; set; }
        public DateTime FirstAbsenceDate { get; set; }
        public DateTime SentAt { get; set; }
        public WarningSentType Type { get; set; }
    }
}
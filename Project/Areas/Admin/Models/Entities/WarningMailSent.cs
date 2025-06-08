using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project.Models.Commons;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("WarningMailSent")]
    public class WarningMailSent : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public Guid TreatmentRecordDetailId { get; set; }
        public DateTime FirstAbsenceDate { get; set; }
        public DateTime SentAt { get; set; }
    }
}
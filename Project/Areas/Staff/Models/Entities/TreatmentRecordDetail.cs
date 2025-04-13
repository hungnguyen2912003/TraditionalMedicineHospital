using Project.Areas.Admin.Models.Entities;
using Project.Models.Commons;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Project.Areas.Staff.Models.Entities
{
    [Table("TreatmentRecordDetail")]
    public class TreatmentRecordDetail : BaseEntity, ICodeEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        public string? Note { get; set; }

        // Foreign keys
        public Guid TreatmentRecordId { get; set; }
        public Guid RoomId { get; set; }

        [ForeignKey("TreatmentRecordId")]
        public virtual TreatmentRecord TreatmentRecord { get; set; } = null!;
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; } = null!;
    }
}
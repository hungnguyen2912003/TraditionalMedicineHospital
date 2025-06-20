﻿using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("TreatmentRecordDetail")]
    public class TreatmentRecordDetail : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        public string? Note { get; set; }
        //Foreign key
        public Guid TreatmentRecordId { get; set; }
        public Guid RoomId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        ///
        [ForeignKey("TreatmentRecordId")]
        public virtual TreatmentRecord TreatmentRecord { get; set; } = null!;
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; } = null!;
        public virtual ICollection<TreatmentTracking> TreatmentTrackings { get; set; } = new List<TreatmentTracking>();
    }
}

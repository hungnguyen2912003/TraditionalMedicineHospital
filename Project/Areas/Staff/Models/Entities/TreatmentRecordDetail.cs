﻿using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project.Areas.Admin.Models.Entities;

namespace Project.Areas.Staff.Models.Entities
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
        public Guid? TreatmentTrackingId { get; set; }
        public Guid RoomId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        ///
        [ForeignKey("TreatmentRecordId")]
        public virtual TreatmentRecord TreatmentRecord { get; set; } = null!;
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; } = null!;
        [ForeignKey("TreatmentTrackingId")]
        public virtual TreatmentTracking? TreatmentTracking { get; set; }
    }
}

﻿using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("Prescription")]
    public class Prescription : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        public DateTime PrescriptionDate { get; set; }
        public string? Note { get; set; }

        //Foreign key
        public Guid TreatmentRecordId { get; set; }
        public Guid EmployeeId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        ///
        [ForeignKey("TreatmentRecordId")]
        public virtual TreatmentRecord? TreatmentRecord { get; set; }
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }
        public virtual ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new HashSet<PrescriptionDetail>();
    }
}

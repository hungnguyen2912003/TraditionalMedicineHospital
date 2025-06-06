using Project.Models.Commons;
using Project.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Project.Areas.Admin.Models.Entities;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("Patient")]
    public class Patient : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        [StringLength(12, MinimumLength = 9)]
        public string? IdentityNumber { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public GenderType Gender { get; set; }
        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;
        [Required]  
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        [EmailAddress]
        public string? EmailAddress { get; set; }
        public string? Images { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        ///
        public virtual HealthInsurance? HealthInsurance { get; set; }
        public virtual User? User { get; set; }
        public virtual ICollection<TreatmentRecord> TreatmentRecords { get; set; } = new HashSet<TreatmentRecord>();
    }
}

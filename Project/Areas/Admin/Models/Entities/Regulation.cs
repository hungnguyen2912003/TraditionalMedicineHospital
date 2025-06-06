using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("Regulation")]
    public class Regulation : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Content { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }


        /////////////////////////////////////////////////////
        /// Relationships
        ///
        public virtual ICollection<TreatmentRecord_Regulation> TreatmentRecord_Regulations { get; set; } = new HashSet<TreatmentRecord_Regulation>();
    }
}

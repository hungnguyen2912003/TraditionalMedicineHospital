using Project.Areas.Admin.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("Room")]
    public class Room : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(10)]
        public string Code { get; set; }
        [Required]
        [StringLength(20)]
        public string Name { get; set; }
        public string? Description { get; set; }

        // ForeignKey
        public Guid TreatmentMethodId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        [ForeignKey("TreatmentMethodId")]
        public virtual TreatmentMethod TreatmentMethod { get; set; }
    }
}

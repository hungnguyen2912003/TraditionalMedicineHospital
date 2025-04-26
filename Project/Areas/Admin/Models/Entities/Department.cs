using Project.Areas.Staff.Models.Entities;
using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("Department")]
    public class Department : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        [Required]
        [StringLength(500)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        ///
        public virtual ICollection<Room> Rooms { get; set; } = new HashSet<Room>();
    }
}

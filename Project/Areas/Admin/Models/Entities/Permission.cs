using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("Permission")]
    public class Permission : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        /////////////////////////////////////////////////////
        /// Relationships
        /// 
        public virtual ICollection<User_Permission> User_Permissions { get; set; } = new HashSet<User_Permission>();
    }
}

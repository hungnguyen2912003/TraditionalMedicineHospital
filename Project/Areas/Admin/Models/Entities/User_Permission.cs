using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("User_Permission")]
    public class User_Permission : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public string? Note { get; set; }

        //Foreign Key
        public Guid UserId { get; set; }
        public Guid PermissionId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        /// 
        [ForeignKey("UserId")]
        public required virtual User User { get; set; }
        [ForeignKey("PermissionId")]
        public required virtual Permission Permission { get; set; }
    }
}

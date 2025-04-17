using Project.Models.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Staff.Models.Entities
{
    [Table("HealthInsurance")]
    public class HealthInsurance : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        public string Number { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string PlaceOfRegistration { get; set; }

        //Foreign key
        public Guid PatientId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        /// 
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; } = null!;
    }
}

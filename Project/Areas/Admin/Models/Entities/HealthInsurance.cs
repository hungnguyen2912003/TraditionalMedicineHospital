using Project.Models.Commons;
using Project.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Areas.Admin.Models.Entities
{
    [Table("HealthInsurance")]
    public class HealthInsurance : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(10)]
        public string Code { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public HealthInsuranceRegistrationPlace PlaceOfRegistration { get; set; }
        public bool IsRightRoute { get; set; }

        //Foreign key
        public Guid PatientId { get; set; }

        /////////////////////////////////////////////////////
        /// Relationships
        /// 
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; } = null!;
    }
}

using Project.Models.Enums;

namespace Project.Areas.NhanVien.Models.DTOs
{
    public class HealthInsuranceDto
    {
        public string Code { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public HealthInsuranceRegistrationPlace PlaceOfRegistration { get; set; }
        public Guid PatientId { get; set; }
        public bool IsRightRoute { get; set; }
    }
}

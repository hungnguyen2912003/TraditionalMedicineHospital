using Project.Models.Enums;

namespace Project.Areas.Staff.Models.DTOs
{
    public class ReceptionPatientDto
    {
        // Thông tin bệnh nhân
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public GenderType Gender { get; set; }
        public string IdentityNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? EmailAddress { get; set; }
        public IFormFile? ImageFile { get; set; }

        // Tùy chọn thẻ BHYT
        public bool HasHealthInsurance { get; set; }

        // Thông tin thẻ BHYT (nếu có)
        public string? HealthInsuranceCode { get; set; }
        public string? HealthInsuranceNumber { get; set; }
        public DateTime? HealthInsuranceExpiryDate { get; set; }
        public string? HealthInsurancePlaceOfRegistration { get; set; }
    }
}

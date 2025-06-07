using Project.Models.Enums;

namespace Project.Areas.NhanVien.Models.ViewModels
{
    public class HealthInsuranceViewModel
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public HealthInsuranceRegistrationPlace PlaceOfRegistration { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsRightRoute { get; set; }
    }
}

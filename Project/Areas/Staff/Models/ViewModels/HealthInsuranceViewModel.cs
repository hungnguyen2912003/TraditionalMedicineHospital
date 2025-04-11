namespace Project.Areas.Staff.Models.ViewModels
{
    public class HealthInsuranceViewModel
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public string PlaceOfRegistration { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}

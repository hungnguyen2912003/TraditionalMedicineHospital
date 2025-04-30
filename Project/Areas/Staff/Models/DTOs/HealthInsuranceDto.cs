namespace Project.Areas.Staff.Models.DTOs
{
    public class HealthInsuranceDto
    {
        public string Code { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public string PlaceOfRegistration { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public bool IsRightRoute { get; set; }
    }
}

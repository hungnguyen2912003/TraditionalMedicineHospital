namespace Project.Areas.BacSi.Models.ViewModels
{
    public class TreatmentSuspendedViewModel
    {
        public string PatientName { get; set; } = string.Empty;
        public string TreatmentCode { get; set; } = string.Empty;
        public DateTime? SuspendedDate { get; set; }
        public string? SuspendedBy { get; set; }
        public string? Reason { get; set; }
    }
}

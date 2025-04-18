using Project.Models.Enums;

namespace Project.Areas.Staff.Models.ViewModels
{
    public class TreatmentRecordViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TreatmentStatus Status { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}

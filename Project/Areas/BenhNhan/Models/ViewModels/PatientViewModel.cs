using Project.Models.Enums;

namespace Project.Areas.BenhNhan.Models.ViewModels
{
    public class PatientViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string TreatmentRecordCode { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string TreatmentMethodName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TreatmentStatus Status { get; set; }
        public string? Note { get; set; }
    }
}

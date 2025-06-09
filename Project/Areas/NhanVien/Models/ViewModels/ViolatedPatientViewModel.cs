using System;

namespace Project.Areas.NhanVien.Models.ViewModels
{
    public class ViolatedPatientViewModel
    {
        public Guid PatientId { get; set; }
        public Guid TreatmentRecordDetailId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? PatientEmail { get; set; }
        public string? PatientPhone { get; set; }
        public DateTime FirstAbsenceDate { get; set; }
        public DateTime SecondAbsenceDate { get; set; }
        public DateTime ThirdAbsenceDate { get; set; }
        public string DepName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
    }
}
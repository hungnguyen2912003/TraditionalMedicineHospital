namespace Project.Areas.Staff.Models.ViewModels
{
    public class WarningPatientViewModel
    {
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? PatientEmail { get; set; }
        public DateTime FirstAbsenceDate { get; set; }
        public DateTime SecondAbsenceDate { get; set; }
        public string? FirstNote { get; set; }
        public string? SecondNote { get; set; }
        public string? DepName { get; set; }
        public string? RoomName { get; set; }
        public string? EmployeeName { get; set; }
    }
}

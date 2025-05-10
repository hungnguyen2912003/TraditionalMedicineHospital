namespace Project.Areas.Staff.Models.ViewModels
{
    public class PrescriptionViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime PrescriptionDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Note { get; set; }
        public Guid TreatmentRecordId { get; set; }
        public string TreatmentRecordCode { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public bool IsActive { get; set; }

    }
}

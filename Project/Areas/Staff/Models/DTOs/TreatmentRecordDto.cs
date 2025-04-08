namespace Project.Areas.Staff.Models.DTOs
{
    public class TreatmentRecordDto
    {
        public string Code { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Note { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
    }
}

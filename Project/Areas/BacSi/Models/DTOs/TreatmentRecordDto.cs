using Project.Models.Enums;

namespace Project.Areas.BacSi.Models.DTOs
{
    public class TreatmentRecordDto
    {
        // Treatment record info
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string Code { get; set; } = string.Empty;
        public DiagnosisType Diagnosis { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Note { get; set; }
        public decimal AdvancePayment { get; set; }
    }
}

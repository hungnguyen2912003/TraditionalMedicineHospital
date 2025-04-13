using Project.Models.Enums;

namespace Project.Areas.Staff.Models.DTOs
{
    public class ReceptionTreatmentRecordDto
    {
        public string Code { get; set; } = string.Empty;
        public DiagnosisType Diagnosis { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

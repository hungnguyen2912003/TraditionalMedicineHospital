using Project.Models.Enums;

namespace Project.Areas.Staff.Models.DTOs.ReceptionDTO
{
    public class ReceptionTreatmentRecordDto
    {
        // Treatment record info
        public string Code { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Note { get; set; }
    }
}

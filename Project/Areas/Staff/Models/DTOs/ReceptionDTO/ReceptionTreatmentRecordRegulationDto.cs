namespace Project.Areas.Staff.Models.DTOs.ReceptionDTO
{
    public class ReceptionTreatmentRecordRegulationDto
    {
        // Treatment record regulation info
        public string Code { get; set; } = string.Empty;
        public Guid RegulationId { get; set; }
        public DateTime ExecutionDate { get; set; }
        public string? Note { get; set; }
    }
}

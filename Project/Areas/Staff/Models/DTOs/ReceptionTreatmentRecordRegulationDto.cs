namespace Project.Areas.Staff.Models.DTOs
{
    public class ReceptionTreatmentRecordRegulationDto
    {
        public string Code { get; set; } = string.Empty;
        public Guid RegulationId { get; set; }
        public DateTime ExecutionDate { get; set; }
        public string? Note { get; set; }
    }
}

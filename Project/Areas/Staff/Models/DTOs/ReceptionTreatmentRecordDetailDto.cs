namespace Project.Areas.Staff.Models.DTOs
{
    public class ReceptionTreatmentRecordDetailDto
    {
        public string Code { get; set; } = string.Empty;
        public Guid TreatmentMethodId { get; set; }
        public Guid RoomId { get; set; }
        public string? Note { get; set; }
    }
}

namespace Project.Areas.Staff.Models.DTOs.ReceptionDTO
{
    public class ReceptionTreatmentRecordDetailDto
    {
        // Treatment record detail info
        public string Code { get; set; } = string.Empty;
        public Guid RoomId { get; set; }
        public string? Note { get; set; }
        public Guid TreatmentRecordId { get; set; }
        public Guid TreatmentMethodId { get; set; }
    }
}

namespace Project.Areas.Staff.Models.DTOs
{
    public class PaymentCreateRequest
    {
        public string Code { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string? Note { get; set; }
        public Guid TreatmentRecordId { get; set; }
    }
}
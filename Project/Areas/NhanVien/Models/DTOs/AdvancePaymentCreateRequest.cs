namespace Project.Areas.NhanVien.Models.DTOs
{
    public class AdvancePaymentCreateRequest
    {
        public string Code { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? Note { get; set; }
        public Guid TreatmentRecordId { get; set; }
    }
}

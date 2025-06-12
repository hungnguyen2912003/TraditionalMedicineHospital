using Project.Areas.Admin.Models.Entities;
using Project.Models.Enums;

namespace Project.Areas.NhanVien.Models.ViewModels
{
    public class AdvancePaymentViewModel
    {
        public Guid Id { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Code { get; set; } = string.Empty;
        public string TreatmentRecordCode { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Note { get; set; }
        public PaymentStatus Status { get; set; }
        public PaymentType? Type { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
using Project.Models.Enums;

namespace Project.Areas.NhanVien.Models.DTOs
{
    public class UpdatePaymentStatusRequest
    {
        public Guid Id { get; set; }
        public PaymentStatus Status { get; set; }
    }
} 
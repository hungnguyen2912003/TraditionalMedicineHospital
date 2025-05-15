using Project.Models.Enums;

namespace Project.Areas.Staff.Models.DTOs
{
    public class UpdatePaymentStatusRequest
    {
        public Guid Id { get; set; }
        public PaymentStatus Status { get; set; }
    }
} 
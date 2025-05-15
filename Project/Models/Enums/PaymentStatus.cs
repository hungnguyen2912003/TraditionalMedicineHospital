using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum PaymentStatus
    {
        [Display(Name = "Chưa thanh toán")]
        ChuaThanhToan = 1,
        [Display(Name = "Đã thanh toán")]
        DaThanhToan = 2,
    }
}
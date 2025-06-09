using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum PaymentType
    {
        [Display(Name = "Thanh toán tại quầy")]
        TrucTiep = 1,
        [Display(Name = "Thanh toán trực tuyến")]
        TrucTuyen = 2 
    }
}

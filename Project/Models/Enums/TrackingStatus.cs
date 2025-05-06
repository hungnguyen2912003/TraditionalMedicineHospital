using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum TrackingStatus
    {
        [Display(Name = "Có khám")]
        CoKham = 1,
        [Display(Name = "Đã xin phép")]
        XinPhep = 2,
        [Display(Name = "Không khám")]
        KhongKham = 3
    }
}

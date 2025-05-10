using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum TrackingStatus
    {
        [Display(Name = "Có điều trị")]
        CoDieuTri = 1,
        [Display(Name = "Đã xin phép")]
        XinPhep = 2,
        [Display(Name = "Không điều trị")]
        KhongDieuTri = 3
    }
}

using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum TreatmentStatus
    {
        [Display(Name = "Đang trong đợt điều trị")]
        DangDieuTri = 1,
        [Display(Name = "Đã hoàn thành đợt điều trị")]
        DaHoanThanh = 2,
        [Display(Name = "Đã kết thúc")]
        DaKetThuc = 3,
        [Display(Name = "Đã hủy bỏ")]
        DaHuyBo = 4
    }
}

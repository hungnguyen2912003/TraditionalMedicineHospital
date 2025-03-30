using System.ComponentModel.DataAnnotations;

namespace Project.Areas.Admin.Models.Enums.Employee
{
    public enum ProfessionalQualificationType
    {
        [Display(Name = "Bác sĩ y học cổ truyền")]
        BacSiYHocCoTruyen = 0,

        [Display(Name = "Dược sĩ đông y")]
        DuocSiDongY = 1,

        [Display(Name = "Chuyên viên châm cứu")]
        ChuyenVienChamCuu = 2,

        [Display(Name = "Chuyên viên vật lý trị liệu")]
        ChuyenVienVatLyTriLieu = 3,

        [Display(Name = "Chuyên viên phục hồi chức năng")]
        ChuyenVienPhucHoiChucNang = 4,

        [Display(Name = "Lương y")]
        LuongY = 5,

        [Display(Name = "Điều dưỡng")]
        DieuDuong = 6,

        [Display(Name = "Y sĩ")]
        YSi = 7,

        [Display(Name = "Y tá")]
        YTa = 8,

        [Display(Name = "Dược sĩ")]
        DuocSi = 9
    }
}

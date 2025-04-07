using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum ProfessionalQualificationType
    {
        [Display(Name = "Bác sĩ y học cổ truyền")]
        BacSiYHocCoTruyen = 1,

        [Display(Name = "Dược sĩ đông y")]
        DuocSiDongY = 2,

        [Display(Name = "Chuyên viên châm cứu")]
        ChuyenVienChamCuu = 3,

        [Display(Name = "Chuyên viên vật lý trị liệu")]
        ChuyenVienVatLyTriLieu = 4,

        [Display(Name = "Chuyên viên phục hồi chức năng")]
        ChuyenVienPhucHoiChucNang = 5,

        [Display(Name = "Lương y")]
        LuongY = 6,

        [Display(Name = "Điều dưỡng")]
        DieuDuong = 7,

        [Display(Name = "Y sĩ")]
        YSi = 8,

        [Display(Name = "Y tá")]
        YTa = 9,

        [Display(Name = "Dược sĩ")]
        DuocSi = 10
    }
}

using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum RoleType
    {
        [Display(Name = "Admin")]
        Admin = 1,
        [Display(Name = "Bác sĩ")]
        BacSi = 2,
        [Display(Name = "Nhân viên Y tá")]
        YTa = 3,
        [Display(Name = "Bệnh nhân")]
        BenhNhan = 4,
        [Display(Name = "Nhân viên hành chính")]
        NhanVienHanhChinh = 5
    }
}

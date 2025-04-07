using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum RoleType
    {
        [Display(Name = "Admin")]
        Admin = 1,
        [Display(Name = "Nhân viên")]
        Nhanvien = 2
    }
}

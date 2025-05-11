using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum RoleType
    {
        [Display(Name = "Admin")]
        Admin = 1,
        [Display(Name = "Bác sĩ")]
        Bacsi = 2,
        [Display(Name = "Nhân viên Y tá")]
        Yta = 3,

        [Display(Name = "Bệnh nhân")]
        Benhnhan = 4,
    }
}

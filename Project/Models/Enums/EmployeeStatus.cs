using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum EmployeeStatus
    {
        [Display(Name = "Đang làm việc")]
        DangLamViec = 1,
        [Display(Name = "Đã nghỉ việc")]
        DaNghiViec = 2,
        [Display(Name = "Nghỉ hưu")]
        NghiHuu = 3
    }
}

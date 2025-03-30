using System.ComponentModel.DataAnnotations;

namespace Project.Areas.Admin.Models.Enums.Employee
{
    public enum EmployeeStatus
    {
        [Display(Name = "Đang làm việc")]
        Working = 0,
        [Display(Name = "Đã nghỉ việc")]
        Resigned = 1,
        [Display(Name = "Nghỉ hưu")]
        Retired = 2
    }
}

using System.ComponentModel.DataAnnotations;

namespace Project.Areas.Admin.Models.Enums.Employee
{
    public enum GenderType
    {
        [Display(Name = "Nam")]
        Nam = 1,
        [Display(Name = "Nữ")]
        Nu = 2,
        [Display(Name = "Khác")]
        Khac = 3
    }
}

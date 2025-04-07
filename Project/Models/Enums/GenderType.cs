using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
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

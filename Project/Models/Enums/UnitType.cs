using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum UnitType
    {
        [Display(Name = "Chai")]
        Chai = 1,
        [Display(Name = "Ống")]
        Ong = 2,
        [Display(Name = "Hộp")]
        Hop = 3,
        [Display(Name = "Gói")]
        Goi = 4,
        [Display(Name = "Viên")]
        Vien = 5,
        [Display(Name = "Lọ")]
        Lo = 6

    }
}

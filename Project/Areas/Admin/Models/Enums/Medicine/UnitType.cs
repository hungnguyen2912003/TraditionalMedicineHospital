using System.ComponentModel.DataAnnotations;

namespace Project.Areas.Admin.Models.Enums.Medicines
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
        Goi = 4
    }
}

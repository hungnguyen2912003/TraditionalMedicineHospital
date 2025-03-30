using System.ComponentModel.DataAnnotations;

namespace Project.Areas.Admin.Models.Enums.Employee
{
    public enum DegreeType
    {
        [Display(Name = "Trung cấp")]
        TrungCap = 0,     
        [Display(Name = "Cao đẳng")]
        CaoDang = 1,   
        [Display(Name = "Đại học")]
        DaiHoc = 2,      
        [Display(Name = "Thạc sĩ")]
        ThacSi = 3,     
        [Display(Name = "Tiến sĩ")]
        TienSi = 4,
        [Display(Name = "Phó Giáo sư")]
        PhoGiaoSu = 5,
        [Display(Name = "Giáo sư")]
        GiaoSu = 6 
    }
}

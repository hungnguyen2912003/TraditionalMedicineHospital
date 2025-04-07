using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum DegreeType
    {
        [Display(Name = "Trung cấp")]
        TrungCap = 1,     
        [Display(Name = "Cao đẳng")]
        CaoDang = 2,   
        [Display(Name = "Đại học")]
        DaiHoc = 3,      
        [Display(Name = "Thạc sĩ")]
        ThacSi = 4,     
        [Display(Name = "Tiến sĩ")]
        TienSi = 5,
        [Display(Name = "Phó Giáo sư")]
        PhoGiaoSu = 6,
        [Display(Name = "Giáo sư")]
        GiaoSu = 7 
    }
}

using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum DiagnosisType
    {
        [Display(Name = "Đau cột sống")]
        DauCotSong = 1,

        [Display(Name = "Đau khớp")]
        DauKhop = 2,

        [Display(Name = "Thoát vị đĩa đệm")]
        ThoatViDiaDem = 3,

        [Display(Name = "Viêm khớp")]
        ViemKhop = 4,

        [Display(Name = "Thoái hóa khớp")]
        ThoaiHoaKhop = 5,

        [Display(Name = "Đau thần kinh tọa")]
        DauThanKinhToa = 6
    }
}
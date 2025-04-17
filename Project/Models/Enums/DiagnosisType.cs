using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum DiagnosisType
    {
        [Display(Name = "Đau cột sống")]
        DauCotSong = 0,

        [Display(Name = "Đau khớp")]
        DauKhop = 1,

        [Display(Name = "Thoát vị đĩa đệm")]
        ThoatViDiaDem = 2,

        [Display(Name = "Viêm khớp")]
        ViemKhop = 3,

        [Display(Name = "Thoái hóa khớp")]
        ThoaiHoaKhop = 4,

        [Display(Name = "Đau thần kinh tọa")]
        DauThanKinhToa = 5
    }
}
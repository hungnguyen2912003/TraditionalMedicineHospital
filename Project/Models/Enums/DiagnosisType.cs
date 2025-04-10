using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum DiagnosisType
    {
        [Display(Name = "Khí huyết lưỡng hư")]
        KhiHuyetLuongHu = 0,
        [Display(Name = "Khí hư")]
        KhiHu = 1,
        [Display(Name = "Huyết hư")]
        HuyetHu = 2,
        [Display(Name = "Huyết ứ")]
        HuyetU = 3,
        [Display(Name = "Khí trệ")]
        KhiTrap = 4,
        [Display(Name = "Can dương thượng kháng")]
        CanDuongThuongKang = 5,
        [Display(Name = "Tỳ hư")]
        TyHu = 6,
        [Display(Name = "Thận dương hư")]
        ThanDuongHu = 7,
        [Display(Name = "Thận âm hư")]
        ThanAmHu = 8,
        [Display(Name = "Phế nhiệt")]
        PheNhiet = 9,
        [Display(Name = "Biểu hàn")]
        BieuHan = 10,
        [Display(Name = "Biểu nhiệt")]
        BieuNhiet = 11,
        [Display(Name = "Lý hàn")]
        LyHan = 12,
        [Display(Name = "Lý nhiệt")]
        LyNhiet = 13,
        [Display(Name = "Phong hàn thấp")]
        PhongHanThap = 14,
        [Display(Name = "Phong nhiệt thấp")]
        PhongNhietThap = 15,
        [Display(Name = "Âm hư nhiệt tích")]
        AmHuNhietTich = 16,
        [Display(Name = "Dương hư hàn tích")]
        DuongHuHanTich = 17
    }
}
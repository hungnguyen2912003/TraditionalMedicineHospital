using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum ManufacturerType
    {
        [Display(Name = "Pfizer (Mỹ)")]
        Pfizer = 1,

        [Display(Name = "Novartis (Thụy Sĩ)")]
        Novartis = 2,

        [Display(Name = "Roche (Thụy Sĩ)")]
        Roche = 3,

        [Display(Name = "Johnson & Johnson (Mỹ)")]
        JohnsonAndJohnson = 4,

        [Display(Name = "Merck & Co. (Mỹ)")]
        Merck = 5,

        [Display(Name = "Sanofi (Pháp)")]
        Sanofi = 6,

        [Display(Name = "AstraZeneca (Anh)")]
        AstraZeneca = 7,

        [Display(Name = "GlaxoSmithKline (Anh)")]
        GlaxoSmithKline = 8,

        [Display(Name = "Bayer (Đức)")]
        Bayer = 9,

        [Display(Name = "Takeda (Nhật Bản)")]
        Takeda = 10,

        [Display(Name = "Abbott (Mỹ)")]
        Abbott = 11,

        [Display(Name = "Dược Hậu Giang (Việt Nam)")]
        DHGPharma = 12,

        [Display(Name = "Traphaco (Việt Nam)")]
        Traphaco = 13,

        [Display(Name = "Pymepharco (Việt Nam)")]
        Pymepharco = 14,
        [Display(Name = "Công ty Đông Dược (Việt Nam)")]
        DongDuoc = 15,
        [Display(Name = "Dược Phẩm Tâm Bình (Việt Nam)")]
        TamBinh = 16,
        [Display(Name = "Dược Phẩm Hoa Linh (Việt Nam)")]
        HoaLinh = 17,
        [Display(Name = "Công ty Dược TW (Việt Nam)")]
        TW = 18,
        [Display(Name = "Dược Phẩm Mekophar (Việt Nam)")]
        Mekophar = 19,
        [Display(Name = "Dược Phẩm Sao Thái Dương (Việt Nam)")]
        SaoThaiDuong = 20
    }
}

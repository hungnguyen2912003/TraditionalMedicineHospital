using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum DiagnosisType
    {
        [Display(Name = "Khí huyết lưỡng hư (Thiếu cả khí và huyết)")]
        KhiHuyetLuongHu = 0,
        [Display(Name = "Khí hư (Thiếu khí, mệt mỏi, thở ngắn)")]
        KhiHu = 1,
        [Display(Name = "Huyết hư (Thiếu máu, da xanh, móng tay nhợt)")]
        HuyetHu = 2,
        [Display(Name = "Huyết ứ (Máu lưu thông kém, đau nhức)")]
        HuyetU = 3,
        [Display(Name = "Khí trệ (Khí không lưu thông, đầy bụng)")]
        KhiTrap = 4,
        [Display(Name = "Can dương thượng kháng (Nóng trong, đau đầu, chóng mặt)")]
        CanDuongThuongKang = 5,
        [Display(Name = "Tỳ hư (Tiêu hóa kém, ăn không ngon)")]
        TyHu = 6,
        [Display(Name = "Thận dương hư (Lạnh chân tay, tiểu đêm)")]
        ThanDuongHu = 7,
        [Display(Name = "Thận âm hư (Nóng trong, mất ngủ)")]
        ThanAmHu = 8,
        [Display(Name = "Phế nhiệt (Ho, khó thở, đờm vàng)")]
        PheNhiet = 9,
        [Display(Name = "Biểu hàn (Cảm lạnh, sợ lạnh)")]
        BieuHan = 10,
        [Display(Name = "Biểu nhiệt (Cảm nóng, sốt, sợ nóng)")]
        BieuNhiet = 11,
        [Display(Name = "Lý hàn (Lạnh trong, đau bụng)")]
        LyHan = 12,
        [Display(Name = "Lý nhiệt (Nóng trong, khát nước)")]
        LyNhiet = 13,
        [Display(Name = "Phong hàn thấp (Đau nhức xương khớp do lạnh)")]
        PhongHanThap = 14,
        [Display(Name = "Phong nhiệt thấp (Đau nhức xương khớp do nóng)")]
        PhongNhietThap = 15,
        [Display(Name = "Âm hư nhiệt tích (Nóng trong, khô miệng)")]
        AmHuNhietTich = 16,
        [Display(Name = "Dương hư hàn tích (Lạnh trong, sợ lạnh)")]
        DuongHuHanTich = 17,
        [Display(Name = "Thoái hóa khớp (Đau khớp, cứng khớp, hạn chế vận động)")]
        ThoaiHoaKhop = 18,
        [Display(Name = "Viêm khớp dạng thấp (Sưng đau khớp, biến dạng khớp)")]
        ViemKhopDangThap = 19,
        [Display(Name = "Đau thần kinh tọa (Đau lan từ thắt lưng xuống chân)")]
        DauThanKinhToa = 20,
        [Display(Name = "Thoát vị đĩa đệm (Đau cột sống, tê bì chân tay)")]
        ThoatViDiaDem = 21,
        [Display(Name = "Liệt nửa người (Yếu liệt một bên cơ thể)")]
        LietNuaNguoi = 22,
        [Display(Name = "Liệt tứ chi (Yếu liệt cả tay và chân)")]
        LietTuChi = 23,
        [Display(Name = "Co cứng cơ (Cơ bắp co cứng, khó vận động)")]
        CoCungCo = 24,
        [Display(Name = "Teo cơ (Cơ bắp yếu, teo nhỏ)")]
        TeoCo = 25,
        [Display(Name = "Đau cơ xơ hóa (Đau lan tỏa toàn thân)")]
        DauCoXoHoa = 26,
        [Display(Name = "Viêm gân (Đau, sưng tại vị trí gân)")]
        ViemGan = 27,
        [Display(Name = "Bong gân (Tổn thương dây chằng)")]
        BongGan = 28,
        [Display(Name = "Gãy xương (Tổn thương xương cần phục hồi)")]
        GayXuong = 29
    }
}
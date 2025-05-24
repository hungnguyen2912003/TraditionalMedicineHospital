using System.ComponentModel.DataAnnotations;

namespace Project.Models.Enums
{
    public enum HealthInsuranceRegistrationPlace
    {
        [Display(Name = "Bệnh viện Trung Ương")]
        BenhVienTrungUong = 1,

        [Display(Name = "Bệnh viện Đa khoa")]
        BenhVienDaKhoa = 2,

        [Display(Name = "Bệnh viện Y học Cổ truyền")]
        BenhVienYHCT = 3,

        [Display(Name = "Bệnh viện Quận/Huyện")]
        BenhVienQuanHuyen = 4,

        [Display(Name = "Phòng khám Đa khoa")]
        PhongKhamDaKhoa = 5,

        [Display(Name = "Trạm Y tế")]
        TramYTe = 6
    }
}
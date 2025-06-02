using System.ComponentModel.DataAnnotations;
using Project.Models.Enums;

namespace Project.Areas.Admin.Models.DTOs
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3-20 ký tự.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-20 ký tự.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
        public RoleType Role { get; set; }

        public Guid? EmployeeId { get; set; }
        public Guid? PatientId { get; set; }
    }
}
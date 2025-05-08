using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Repositories.Interfaces;
using Project.Services.Features;

namespace Project.Areas.Admin.Controllers.Api
{
    [Area("Admin")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtManager _jwtManager;

        public UserController(IUserRepository userRepository, JwtManager jwtManager)
        {
            _userRepository = userRepository;
            _jwtManager = jwtManager;
        }

        [HttpGet("GetInfo")]
        public async Task<IActionResult> GetInfo()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { success = false, message = "Chưa đăng nhập." });
            }

            var (username, role) = _jwtManager.GetClaimsFromToken(token);
            if (string.IsNullOrEmpty(username))
            {
                Response.Cookies.Delete("AuthToken");
                return Unauthorized(new { success = false, message = "Token không hợp lệ." });
            }

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy người dùng." });
            }

            if (role == "Admin")
            {
                return Ok(new
                {
                    success = true,
                    username = username,
                    role = role
                });
            }
            else
            {
                var employee = user.Employee;
                if (employee == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy thông tin nhân viên." });
                }

                string imagePath = string.IsNullOrEmpty(employee.Images)
                    ? ""
                    : $"/Images/Employees/{employee.Images}";

                return Ok(new
                {
                    success = true,
                    code = employee.Code,
                    id = employee.Id,
                    username = username,
                    name = employee.Name,
                    email = employee.EmailAddress,
                    categoryName = employee.EmployeeCategory != null ? employee.EmployeeCategory.Name : null,
                    departmentName = (employee.Room != null && employee.Room.Department != null) ? employee.Room.Department.Name : null,
                    roomName = employee.Room != null ? employee.Room.Name : null,
                    image = imagePath,
                    role = role
                });
            }
        }
    }
}
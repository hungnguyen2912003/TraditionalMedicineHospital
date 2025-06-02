using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using Project.Areas.Admin.Models.DTOs;
using Project.Models.Enums;
using Project.Areas.Admin.Models.Entities;
using BCrypt.Net;
using Project.Models.DTOs;

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

        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userRepository.GetAllAdvancedAsync();
            var result = users.Select(u => new
            {
                id = u.Id,
                username = u.Username,
                role = u.Role,
                employeeName = u.Employee != null ? u.Employee.Name : null,
                patientName = u.Patient != null ? u.Patient.Name : null,
                createdBy = u.CreatedBy,
                createdDate = u.CreatedDate
            }).OrderBy(u => u.role).ThenBy(u => u.username).ToList();

            return Ok(result);
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
            else if (role == "Bacsi" || role == "Yta")
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
            else if (role == "Benhnhan")
            {
                var patient = user.Patient;
                if (patient == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy thông tin bệnh nhân." });
                }

                string imagePath = string.IsNullOrEmpty(patient.Images)
                    ? ""
                    : $"/Images/Patients/{patient.Images}";

                return Ok(new
                {
                    success = true,
                    code = patient.Code,
                    id = patient.Id,
                    username = username,
                    name = patient.Name,
                    email = patient.EmailAddress,
                    image = imagePath,
                    role = role
                });
            }
            else
            {
                return Unauthorized(new { success = false, message = "Không có quyền truy cập." });
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            // Check if username already exists
            var existingUser = await _userRepository.GetByUsernameAsync(dto.Username);
            if (existingUser != null)
            {
                return BadRequest(new { success = false, message = "Tên đăng nhập đã tồn tại." });
            }

            // Create password hash using BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                PasswordHash = passwordHash,
                Role = RoleType.Admin,
                EmployeeId = null,
                PatientId = null,
                IsFirstLogin = false,
                CreatedBy = "Admin",
                CreatedDate = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user);

            return Ok(new { success = true, message = "Tạo tài khoản thành công." });
        }

        [HttpPost("Update/{id}")]
        public async Task<IActionResult> Update([FromBody] UpdateUserDto dto, Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { success = false, message = "Không tìm thấy tài khoản." });

            // Nếu username mới khác username cũ, kiểm tra trùng
            if (!string.Equals(user.Username, dto.Username, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _userRepository.GetByUsernameAsync(dto.Username);
                if (existing != null)
                {
                    return BadRequest(new { success = false, message = "Tên đăng nhập đã tồn tại." });
                }
            }

            user.Username = dto.Username;
            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                user.IsFirstLogin = false;
            }
            user.UpdatedBy = "Admin";
            user.UpdatedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return Ok(new { success = true, message = "Cập nhật tài khoản thành công." });
        }

        [HttpGet("GetById/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound();
            return Ok(new
            {
                username = user.Username
            });
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using System.Security.Claims;

namespace TraditionalMedicineHospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtManager _jwtManager;

        public AuthController(IUserRepository userRepository, JwtManager jwtManager)
        {
            _userRepository = userRepository;
            _jwtManager = jwtManager;
        }

        [Authorize]
        [HttpGet("GetCurrentDoctor")]
        public async Task<IActionResult> GetCurrentDoctor()
        {
            try 
            {
                if (User.Identity?.Name == null)
                {
                    return Unauthorized(new { success = false, message = "Chưa đăng nhập." });
                }

                var username = User.Identity.Name;
                var user = await _userRepository.GetByUsernameAsync(username);
                
                if (user == null || user.Employee == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy thông tin bác sĩ" });
                }

                return Ok(new
                {
                    success = true,
                    id = user.Employee.Id,
                    name = user.Employee.Name,
                    userId = user.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }
    }
} 
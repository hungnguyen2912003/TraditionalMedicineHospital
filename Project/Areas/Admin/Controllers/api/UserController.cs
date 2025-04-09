using Microsoft.AspNetCore.Mvc;
using Project.Repositories.Interfaces;
using Project.Services.Features;

namespace Project.Areas.Admin.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtManager _jwtManager;

        public UserController(IUserRepository userRepository, JwtManager jwtManager)
        {
            _userRepository = userRepository;
            _jwtManager = jwtManager;
        }

        [HttpGet("user-info")]
        public async Task<IActionResult> GetUserProfile()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Không tìm thấy token.");
            }

            var (username, role) = _jwtManager.GetClaimsFromToken(token);

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
            {
                return Unauthorized("Token không hợp lệ.");
            }

            var user = await _userRepository.GetByUsernameAsync(username);

            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            var response = new
            {
                Username = user.Username,
                Role = role,
                FullName = user.Employee?.Name,
                Email = user.Employee?.EmailAddress,
                Image = user.Employee?.Images
            };

            return Ok(response);
        }
    }
}

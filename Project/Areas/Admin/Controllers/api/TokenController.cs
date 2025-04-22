using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Models.Enums;
using Project.Services.Features;

namespace Project.Areas.Admin.Controllers.Api
{
    [Area("Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly JwtManager _jwtManager;

        public TokenController(JwtManager jwtManager)
        {
            _jwtManager = jwtManager;
        }

        [Authorize]
        [HttpGet("Renew")]
        public IActionResult RenewToken()
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

            if (!Enum.TryParse<RoleType>(role, out var roleType))
            {
                return BadRequest(new { success = false, message = "Vai trò không hợp lệ." });
            }

            var newToken = _jwtManager.GenerateToken(username, roleType);
            Response.Cookies.Append("AuthToken", newToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddHours(1)
            });

            return Ok(new { success = true, message = "Token đã được làm mới." });
        }
    }
}
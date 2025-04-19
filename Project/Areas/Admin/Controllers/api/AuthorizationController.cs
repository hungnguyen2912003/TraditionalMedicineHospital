using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Project.Areas.Admin.Controllers.Api
{
    [Area("Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        [HttpGet("AccessDenied")]
        public IActionResult AccessDenied()
        {
            return Unauthorized(new { success = false, message = "Không có quyền truy cập." });
        }
    }
}
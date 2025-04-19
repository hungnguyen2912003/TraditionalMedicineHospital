using Microsoft.AspNetCore.Mvc;
using Project.Helpers;
using System.Runtime.Versioning;

namespace Project.Areas.Admin.Controllers.Api
{
    [Area("Admin")]
    [Route("api/[controller]")]
    [ApiController]
    [SupportedOSPlatform("windows")]
    public class CaptchaController : ControllerBase
    {
        private readonly CaptchaHelper _captchaHelper;

        public CaptchaController(CaptchaHelper captchaHelper)
        {
            _captchaHelper = captchaHelper;
        }

        [HttpGet("GetImage")]
        public IActionResult GetImage()
        {
            return _captchaHelper.GetCaptchaImage(HttpContext);
        }
    }
}
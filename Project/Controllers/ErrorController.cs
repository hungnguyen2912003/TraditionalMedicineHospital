using Microsoft.AspNetCore.Mvc;

namespace Project.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HandleError(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    return View("Error404");
                case 500:
                    return View("Error500");
                case 503:
                    return View("Error503");
                default:
                    return View("Error");
            }
        }
    }
}

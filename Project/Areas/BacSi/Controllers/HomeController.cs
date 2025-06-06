using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace Project.Areas.BacSi.Controllers
{
    [Area("BacSi")]
    [Authorize(Roles = "BacSi")]
    [Route("bac-si")]
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}

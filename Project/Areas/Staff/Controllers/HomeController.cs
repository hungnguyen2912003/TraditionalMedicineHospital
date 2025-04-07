using Microsoft.AspNetCore.Mvc;

namespace Project.Areas.Staff.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

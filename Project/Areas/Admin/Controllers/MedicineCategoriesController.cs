using Microsoft.AspNetCore.Mvc;
using Project.Repositories.Interfaces;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MedicineCategoriesController : Controller
    {
        public MedicineCategoriesController(IMedicineCategoryRepository repository)
        {

        }
    }
}

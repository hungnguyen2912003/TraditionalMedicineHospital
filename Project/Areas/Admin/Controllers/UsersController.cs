using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Repositories.Interfaces;
using Project.Areas.Admin.Models.ViewModels;
using Project.Helpers;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ViewBagHelper _viewBagHelper;

        public UsersController(IUserRepository userRepository, IMapper mapper, ViewBagHelper viewBagHelper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _viewBagHelper = viewBagHelper;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAllAdvancedAsync();
            var viewModel = users.Select(u => new UserViewModel
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role,
                EmployeeName = u.Employee != null ? u.Employee.Name : null,
                IsActive = u.IsActive,
                CreatedBy = u.CreatedBy,
                CreatedDate = u.CreatedDate
            }).ToList();
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(viewModel);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var user = (await _userRepository.GetAllAdvancedAsync()).FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            var viewModel = new UserViewModel
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                EmployeeName = user.Employee != null ? user.Employee.Name : null,
                IsActive = user.IsActive,
                CreatedBy = user.CreatedBy,
                CreatedDate = user.CreatedDate
            };
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(viewModel);
        }
    }
}
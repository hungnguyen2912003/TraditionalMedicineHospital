using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.Admin.Models.ViewModels;
using Project.Helpers;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using Project.Services.Interfaces;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("nhan-su")]
    public class EmployeesController : Controller
    {
        private readonly IEmployeeRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IImageService _imgService;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly CodeGeneratorHelper _codeGenerator;
        private readonly IRoomRepository _roomRepository;
        private readonly EmailService _emailService;
        public EmployeesController
        (
            IEmployeeRepository repository,
            IUserRepository userRepository,
            IMapper mapper,
            IImageService imgService,
            ViewBagHelper viewBagHelper,
            CodeGeneratorHelper codeGenerator,
            IRoomRepository roomRepository,
            EmailService emailService
        )
        {
            _repository = repository;
            _userRepository = userRepository;
            _mapper = mapper;
            _imgService = imgService;
            _viewBagHelper = viewBagHelper;
            _codeGenerator = codeGenerator;
            _roomRepository = roomRepository;
            _emailService = emailService;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var list = await _repository.GetAllAdvancedAsync();
            var viewModelList = _mapper.Map<List<EmployeeViewModel>>(list);
            viewModelList = viewModelList.OrderBy(x => x.DepartmentName).ToList();
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(viewModelList);
        }

        [Authorize(Roles = "Admin, Bacsi, Yta")]
        [Route("chi-tiet/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var entity = await _repository.GetByIdAdvancedAsync(id);
            if (entity == null)
            {
                return NotFound();
            }
            return View(entity);
        }

        [HttpGet("them-moi")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await _viewBagHelper.BaseViewBag(ViewData);
            var model = new EmployeeDto
            {
                Code = await _codeGenerator.GenerateNumericCodeAsync(_repository)
            };

            return View(model);
        }

        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] EmployeeDto inputDto)
        {
            try
            {
                var entity = _mapper.Map<Employee>(inputDto);
                entity.CreatedBy = "Admin";
                entity.Status = EmployeeStatus.DangLamViec;
                entity.CreatedDate = DateTime.UtcNow;

                if (inputDto.ImageFile != null && inputDto.ImageFile.Length > 0)
                {
                    entity.Images = await _imgService.SaveImageAsync(inputDto.ImageFile, "Employees");
                }

                await _repository.CreateAsync(entity);

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = entity.Code,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("11111111"),
                    Role = entity.EmployeeCategory?.Name?.ToLower().Contains("y tá") == true ? RoleType.Yta : RoleType.Bacsi,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "Admin",
                    EmployeeId = entity.Id,
                    IsFirstLogin = true
                };

                await _userRepository.CreateAsync(user);

                if (!string.IsNullOrEmpty(entity.EmailAddress))
                {
                    var subject = "Tài khoản nhân viên mới đã được đăng ký trên hệ thống Bệnh viện Y học cổ truyền Nha Trang";
                    var body = $@"
                        <h2>Xin chào {entity.Name},</h2>
                        <p>Bạn đã được cấp tài khoản nhân viên tại hệ thống Bệnh viện Y học cổ truyền Nha Trang.</p>
                        <p>Tài khoản của bạn là:</p>
                        <p><b>Mã nhân viên (Username):</b> {entity.Code}</p>
                        <p><b>Mật khẩu mặc định:</b> 11111111</p>
                        <p>Bạn có thể sử dụng mã nhân viên hoặc email để đăng nhập vào hệ thống.</p>
                        <p>Vui lòng đăng nhập tại <a href='https://localhost:5285/login'>trang đăng nhập</a> và đổi mật khẩu ngay lần đầu đăng nhập.</p>
                        <p>Trân trọng,<br>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>
                    ";
                    _ = Task.Run(() => _emailService.SendEmailAsync(entity.EmailAddress, subject, body));
                }

                return Json(new { success = true, message = "Thêm nhân sự thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm nhân sự: " + ex.Message });
            }
        }

        [HttpGet("chinh-sua/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var entity = await _repository.GetByIdAdvancedAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<EmployeeDto>(entity);

            ViewBag.EmployeeId = entity.Id;
            ViewBag.ExistingImage = entity.Images;

            await _viewBagHelper.BaseViewBag(ViewData);

            return View(dto);
        }

        [HttpPost("chinh-sua/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] EmployeeDto inputDto, Guid Id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(Id);
                if (entity == null) return NotFound();

                _mapper.Map(inputDto, entity);
                entity.UpdatedBy = "Admin";
                entity.UpdatedDate = DateTime.UtcNow;

                if (inputDto.ImageFile != null && inputDto.ImageFile.Length > 0)
                {
                    entity.Images = await _imgService.SaveImageAsync(inputDto.ImageFile, "Employees");
                }

                await _repository.UpdateAsync(entity);
                return Json(new { success = true, message = "Cập nhật nhân sự thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật nhân sự: " + ex });
            }
        }

        [HttpPost("xoa")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromForm] string selectedIds)
        {
            var ids = new List<Guid>();
            foreach (var id in selectedIds.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (Guid.TryParse(id, out var parsedId))
                {
                    ids.Add(parsedId);
                }
            }

            if (_repository == null)
            {
                TempData["ErrorMessage"] = "Hệ thống gặp lỗi, vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }

            var delList = new List<Employee>();
            foreach (var id in ids)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    if (!string.IsNullOrEmpty(entity.Images))
                    {
                        _imgService.DeleteImage(entity.Images, "Employees");
                    }
                    await _repository.DeleteAsync(id);
                    delList.Add(entity);
                }
            }

            if (delList.Any())
            {
                var names = string.Join(", ", delList.Select(c => $"\"{c.Name}\""));
                var message = delList.Count == 1
                    ? $"Đã xóa vĩnh viễn nhân sự {names} thành công"
                    : $"Đã xóa vĩnh viễn các nhân sự đã chọn thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy nhân sự nào để xóa.";
            }

            return RedirectToAction("Index");
        }
    }
}

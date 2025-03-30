using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.Admin.Models.Enums.Employee;
using Project.Extensions;
using Project.Repositories.Interfaces;
using Project.Services.Interfaces;
using Project.Validators;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EmployeesController : Controller
    {
        private readonly IEmployeeRepository _repository;
        private readonly IEmployeeCategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IImageService _service;
        private readonly IValidator<EmployeeDto> _validator;
        public EmployeesController
        (
            IEmployeeRepository repository,
            IMapper mapper,
            IImageService service,
            IValidator<EmployeeDto> validator,
            IEmployeeCategoryRepository categoryRepository
        )
        {
            _repository = repository;
            _mapper = mapper;
            _service = service;
            _validator = validator;
            _categoryRepository = categoryRepository;
        }
        public async Task<IActionResult> Index()
        {
            var list = await _repository.GetAllWithCategoryAsync();
            var activeList = list.Where(x => x.IsActive == true).ToList();
            var dtoList = _mapper.Map<List<EmployeeDto>>(activeList);
            return View(dtoList);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var entity = await _repository.GetByIdWithCategoryAsync(id);
            if (entity == null)
            {
                return NotFound();
            }
            return View(entity);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var employeeCategories = await _categoryRepository.GetAllActiveAsync();
            ViewBag.EmployeeCategories = employeeCategories
                .Where(mc => mc.IsActive)
                .Select(mc => new { mc.Id, mc.Name })
                .ToList();

            ViewBag.GenderType = Enum.GetValues(typeof(GenderType))
                .Cast<GenderType>()
                .Select(e => new
                {
                    Value = (int)e,
                    Text = e.GetDisplayName()
                })
                .ToList();
            ViewBag.EmployeeStatus = Enum.GetValues(typeof(EmployeeStatus))
                .Cast<EmployeeStatus>()
                .Select(e => new
                {
                    Value = (int)e,
                    Text = e.GetDisplayName()
                })
                .ToList();
            ViewBag.DegreeType = Enum.GetValues(typeof(DegreeType))
                .Cast<DegreeType>()
                .Select(e => new
                {
                    Value = (int)e,
                    Text = e.GetDisplayName()
                })
                .ToList();
            ViewBag.ProfessionalQualificationType = Enum.GetValues(typeof(ProfessionalQualificationType))
                .Cast<ProfessionalQualificationType>()
                .Select(e => new
                {
                    Value = (int)e,
                    Text = e.GetDisplayName()
                })
                .ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] EmployeeDto inputDto)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(inputDto);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToList();
                    return Json(new { success = false, message = "Thêm nhân sự thất bại. Vui lòng kiểm tra lại thông tin.", errors });
                }

                var entity = _mapper.Map<Employee>(inputDto);
                entity.CreatedBy = "Admin";
                entity.CreatedDate = DateTime.UtcNow;
                entity.IsActive = true;

                if (inputDto.ImageFile != null && inputDto.ImageFile.Length > 0)
                {
                    entity.Image = await _service.SaveImageAsync(inputDto.ImageFile, "Employees");
                }

                await _repository.CreateAsync(entity);
                return Json(new { success = true, message = "Thêm nhân sự thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm nhân sự: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<EmployeeDto>(entity);
            ViewBag.EmployeeId = entity.Id;
            ViewBag.ExistingImage = entity.Image;
            var employeeCategories = await _categoryRepository.GetAllActiveAsync();
            ViewBag.EmployeeCategories = employeeCategories
                .Where(mc => mc.IsActive)
                .Select(mc => new { mc.Id, mc.Name })
                .ToList();
            ViewBag.GenderType = Enum.GetValues(typeof(GenderType))
                .Cast<GenderType>()
                .Select(e => new
                {
                    Value = (int)e,
                    Text = e.GetDisplayName()
                })
                .ToList();
            ViewBag.EmployeeStatus = Enum.GetValues(typeof(EmployeeStatus))
                .Cast<EmployeeStatus>()
                .Select(e => new
                {
                    Value = (int)e,
                    Text = e.GetDisplayName()
                })
                .ToList();
            ViewBag.DegreeType = Enum.GetValues(typeof(DegreeType))
                .Cast<DegreeType>()
                .Select(e => new
                {
                    Value = (int)e,
                    Text = e.GetDisplayName()
                })
                .ToList();
            ViewBag.ProfessionalQualificationType = Enum.GetValues(typeof(ProfessionalQualificationType))
                .Cast<ProfessionalQualificationType>()
                .Select(e => new
                {
                    Value = (int)e,
                    Text = e.GetDisplayName()
                })
                .ToList();

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] EmployeeDto inputDto, Guid Id)
        {
            try
            {
                var validator = new EmployeeValidator(_repository, Id);
                var validationResult = await validator.ValidateAsync(inputDto);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => $"{e.ErrorMessage}").ToList();
                    return Json(new { success = false, message = "Cập nhật nhân sự thất bại. Vui lòng kiểm tra lại thông tin.", errors });
                }

                var entity = await _repository.GetByIdAsync(Id);
                if (entity == null) return NotFound();
                entity = _mapper.Map(inputDto, entity);
                entity.UpdatedBy = "Admin";
                entity.UpdatedDate = DateTime.UtcNow;
                if (inputDto.ImageFile != null && inputDto.ImageFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(entity.Image))
                    {
                        _service.DeleteImage(entity.Image, "Employees");
                    }
                    entity.Image = await _service.SaveImageAsync(inputDto.ImageFile, "Employees");
                }
                await _repository.UpdateAsync(entity);
                return Json(new { success = true, message = "Cập nhật nhân sự thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật nhân sự: " + ex.Message });
            }
        }

        public async Task<IActionResult> Trash()
        {
            var list = await _repository.GetAllWithCategoryAsync();
            var trashList = list.Where(x => x.IsActive == false).ToList();
            var dtoList = _mapper.Map<List<EmployeeDto>>(trashList);
            return View(dtoList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                return RedirectToAction("Trash");
            }

            var deletedEmployees = new List<Employee>();
            foreach (var id in ids)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    if (!string.IsNullOrEmpty(entity.Image))
                    {
                        _service.DeleteImage(entity.Image, "Employees");
                    }
                    await _repository.DeleteAsync(id);
                    deletedEmployees.Add(entity);
                }
            }

            if (deletedEmployees.Any())
            {
                var names = string.Join(", ", deletedEmployees.Select(c => $"\"{c.FullName}\""));
                var message = deletedEmployees.Count == 1
                    ? $"Đã xóa vĩnh viễn nhân sự {names} thành công"
                    : $"Đã xóa vĩnh viễn các nhân sự {names} thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy nhân sự nào để xóa.";
            }

            return RedirectToAction("Trash");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveToTrash([FromForm] string selectedIds)
        {
            var ids = new List<Guid>();
            foreach (var id in selectedIds.Split(','))
            {
                if (Guid.TryParse(id, out var parsedId))
                {
                    ids.Add(parsedId);
                }
            }

            var deletedEmployees = new List<Employee>();
            foreach (var id in ids)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    entity.IsActive = false;
                    entity.UpdatedBy = "Admin";
                    entity.UpdatedDate = DateTime.UtcNow;
                    await _repository.UpdateAsync(entity);
                    deletedEmployees.Add(entity);
                }
            }

            if (deletedEmployees.Any())
            {
                var names = string.Join(", ", deletedEmployees.Select(c => $"\"{c.FullName}\""));
                var message = deletedEmployees.Count == 1
                    ? $"Đã đưa nhân sự {names} thành công vào thùng rác"
                    : $"Đã đưa các nhân sự {names} thành công vào thùng rác";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy nhân sự nào để đưa vào thùng rác.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore([FromForm] string selectedIds)
        {
            var ids = new List<Guid>();
            foreach (var id in selectedIds.Split(','))
            {
                if (Guid.TryParse(id, out var parsedId))
                {
                    ids.Add(parsedId);
                }
            }
            var deletedEmployees = new List<Employee>();
            foreach (var id in ids)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    entity.IsActive = true;
                    entity.UpdatedBy = "Admin";
                    entity.UpdatedDate = DateTime.UtcNow;
                    await _repository.UpdateAsync(entity);
                    deletedEmployees.Add(entity);
                }
            }

            if (deletedEmployees.Any())
            {
                var names = string.Join(", ", deletedEmployees.Select(c => $"\"{c.FullName}\""));
                var message = deletedEmployees.Count == 1
                    ? $"Đã khôi phục nhân sự {names} thành công."
                    : $"Đã khôi phục các nhân sự {names} thành công.";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy nhân sự nào để khôi phục.";
            }

            return RedirectToAction("Trash");
        }
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Helpers;
using Project.Repositories.Interfaces;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EmployeeCategoriesController : Controller
    {
        private readonly IEmployeeCategoryRepository _repository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly CodeGeneratorHelper _codeGenerator;
        public EmployeeCategoriesController
        (
            IEmployeeCategoryRepository repository,
            IEmployeeRepository employeeRepository,
            IMapper mapper,
            CodeGeneratorHelper codeGenerator
        )
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
            _codeGenerator = codeGenerator;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _repository.GetAllAsync();
            var activeList = list.Where(x => x.IsActive == true).ToList();
            return View(activeList);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return NotFound();
            return View(entity);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new EmployeeCategoryDto
            {
                Code = await _codeGenerator.GenerateUniqueCodeAsync(_repository)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] EmployeeCategoryDto inputDto)
        {
            try
            {
                var entity = _mapper.Map<EmployeeCategory>(inputDto);

                entity.CreatedBy = "Admin";
                entity.CreatedDate = DateTime.UtcNow;
                entity.IsActive = true;

                await _repository.CreateAsync(entity);
                return Json(new { success = true, message = "Thêm loại nhân sự thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm loại nhân sự: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<EmployeeCategoryDto>(entity);

            ViewBag.EmployeeCategoryId = entity.Id;

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] EmployeeCategoryDto inputDto, Guid Id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(Id);
                if (entity == null) return NotFound();

                _mapper.Map(inputDto, entity);
                entity.UpdatedBy = "Admin";
                entity.UpdatedDate = DateTime.UtcNow;

                await _repository.UpdateAsync(entity);
                return Json(new { success = true, message = "Cập nhật loại nhân sự thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật loại nhân sự: " + ex.Message });
            }
        }

        public async Task<IActionResult> Trash()
        {
            var list = await _repository.GetAllAsync();
            var trashList = list.Where(x => x.IsActive == false).ToList();
            return View(trashList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] string selectedIds)
        {
            var ids = new List<Guid>();
            foreach (var id in selectedIds.Split(','))
            {
                if (Guid.TryParse(id, out var parsedId))
                {
                    ids.Add(parsedId);
                }
            }

            var categories = new List<EmployeeCategory>();
            foreach (var id in ids)
            {
                var category = await _repository.GetByIdAsync(id);
                if (category == null) continue;
                var employees = await _employeeRepository.GetAllAdvancedAsync();
                var hasMedicines = employees.Any(m => m.EmployeeCategoryId == id);
                if (hasMedicines)
                {
                    categories.Add(category);
                }
            }

            if (categories.Any())
            {
                var names = string.Join(", ", categories.Select(c => $"\"{c.Name}\""));
                var message = categories.Count == 1
                    ? $"Không thể xóa loại nhân sự {names} vì vẫn còn nhân sự đang sử dụng loại này."
                    : $"Không thể xóa các loại nhân sự: {names} vì vẫn còn nhân sự đang sử dụng các loại này.";
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Index");
            }

            var delList = new List<EmployeeCategory>();
            foreach (var id in ids)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    await _repository.DeleteAsync(id);
                    delList.Add(entity);
                }
            }

            if (delList.Any())
            {
                var names = string.Join(", ", delList.Select(c => $"\"{c.Name}\""));
                var message = delList.Count == 1
                    ? $"Đã xóa loại nhân sự {names} thành công"
                    : $"Đã xóa các loại nhân sự: {names} thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy loại nhân sự nào để xóa.";
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

            var categories = new List<EmployeeCategory>();
            foreach (var id in ids)
            {
                var category = await _repository.GetByIdAsync(id);
                if (category == null) continue;

                var employees = await _employeeRepository.GetAllAdvancedAsync();
                var hasMedicines = employees.Any(m => m.EmployeeCategoryId == id);

                if (hasMedicines)
                {
                    categories.Add(category);
                }
            }

            if (categories.Any())
            {
                var names = string.Join(", ", categories.Select(c => $"\"{c.Name}\""));
                var message = categories.Count == 1
                    ? $"Không thể đưa loại nhân sự {names} vào thùng rác vì vẫn còn nhân sự đang sử dụng loại này."
                    : $"Không thể đưa các loại nhân sự: {names} vào thùng rác vì vẫn còn nhân sự đang sử dụng các loại này.";
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Index");
            }

            var movedList = new List<EmployeeCategory>();
            foreach (var id in ids)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    entity.IsActive = false;
                    entity.UpdatedBy = "Admin";
                    entity.UpdatedDate = DateTime.UtcNow;
                    await _repository.UpdateAsync(entity);
                    movedList.Add(entity);
                }
            }

            if (movedList.Any())
            {
                var names = string.Join(", ", movedList.Select(c => $"\"{c.Name}\""));
                var message = movedList.Count == 1
                    ? $"Đã đưa loại nhân sự {names} thành công vào thùng rác"
                    : $"Đã đưa các loại nhân sự: {names} thành công vào thùng rác";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy loại nhân sự nào để đưa vào thùng rác.";
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
            var restoredEntity = new List<EmployeeCategory>();
            foreach (var id in ids)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    entity.IsActive = true;
                    entity.UpdatedBy = "Admin";
                    entity.UpdatedDate = DateTime.UtcNow;
                    await _repository.UpdateAsync(entity);
                    restoredEntity.Add(entity);
                }
            }

            if (restoredEntity.Any())
            {
                var names = string.Join(", ", restoredEntity.Select(c => $"\"{c.Name}\""));
                var message = restoredEntity.Count == 1
                    ? $"Đã khôi phục loại nhân sự {names} thành công."
                    : $"Đã khôi phục các loại nhân sự: {names} thành công.";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy loại nhân sự nào để khôi phục.";
            }

            return RedirectToAction("Trash");
        }
    }
}

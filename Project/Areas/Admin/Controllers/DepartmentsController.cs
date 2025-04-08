using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Repositories.Interfaces;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DepartmentsController : Controller
    {
        private readonly IDepartmentRepository _repository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ITreatmentMethodRepository _treatmentRepository;
        private readonly IMapper _mapper;
        public DepartmentsController
        (
            IDepartmentRepository repository,
            IEmployeeRepository employeeRepository,
            ITreatmentMethodRepository treatmentRepository,
            IMapper mapper
        )
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
            _treatmentRepository = treatmentRepository;
            _mapper = mapper;
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
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] DepartmentDto inputDto)
        {
            try
            {
                var entity = _mapper.Map<Department>(inputDto);

                entity.CreatedBy = "Admin";
                entity.CreatedDate = DateTime.UtcNow;
                entity.IsActive = true;

                await _repository.CreateAsync(entity);
                return Json(new { success = true, message = "Thêm khoa thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm khoa: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<DepartmentDto>(entity);

            ViewBag.DepartmentId = entity.Id;

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] DepartmentDto inputDto, Guid Id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(Id);
                if (entity == null) return NotFound();

                _mapper.Map(inputDto, entity);
                entity.UpdatedBy = "Admin";
                entity.UpdatedDate = DateTime.UtcNow;

                await _repository.UpdateAsync(entity);
                return Json(new { success = true, message = "Cập nhật Khoa thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật Khoa: " + ex.Message });
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

            var departments = new List<Department>();
            foreach (var id in ids)
            {
                var dep = await _repository.GetByIdAsync(id);
                if (dep == null) continue;
                var e = await _employeeRepository.GetAllAdvancedAsync();
                var t = await _treatmentRepository.GetAllAdvancedAsync();
                var hasEmployees = e.Any(m => m.DepartmentId == id);
                var hasTreatments = e.Any(m => m.DepartmentId == id);
                if (hasEmployees || hasTreatments)
                {
                    departments.Add(dep);
                }
            }

            if (departments.Any())
            {
                var names = string.Join(", ", departments.Select(c => $"\"{c.Name}\""));
                var message = departments.Count == 1
                    ? $"Không thể xóa Khoa {names} vì vẫn còn nhân sự hoặc phương pháp điều trị đang sử dụng Khoa này."
                    : $"Không thể xóa các Khoa: {names} vì vẫn còn các nhân sự hoặc phương pháp điều trị đang sử dụng Khoa này.";
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Index");
            }

            var delList = new List<Department>();
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
                    ? $"Đã xóa Khoa {names} thành công"
                    : $"Đã xóa các Khoa: {names} thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy Khoa nào để xóa.";
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

            var departments = new List<Department>();
            foreach (var id in ids)
            {
                var dep = await _repository.GetByIdAsync(id);
                if (dep == null) continue;
                var e = await _employeeRepository.GetAllAdvancedAsync();
                var t = await _treatmentRepository.GetAllAdvancedAsync();
                var hasEmployees = e.Any(m => m.DepartmentId == id);
                var hasTreatments = t.Any(m => m.DepartmentId == id);
                if (hasEmployees || hasTreatments)
                {
                    departments.Add(dep);
                }
            }

            if (departments.Any())
            {
                var names = string.Join(", ", departments.Select(c => $"\"{c.Name}\""));
                var message = departments.Count == 1
                    ? $"Không thể đưa Khoa {names} vào thùng rác vì vẫn còn nhân sự hoặc phương pháp điều trị đang sử dụng Khoa này."
                    : $"Không thể đưa các Khoa: {names} vào thùng rác vì vẫn còn các nhân sự hoặc phương pháp điều trị đang sử dụng Khoa này.";
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Index");
            }

            var movedList = new List<Department>();
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
                    ? $"Đã đưa Khoa {names} thành công vào thùng rác"
                    : $"Đã đưa các Khoa: {names} thành công vào thùng rác";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy Khoa nào để đưa vào thùng rác.";
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
            var restoredEntity = new List<Department>();
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
                    ? $"Đã khôi phục Khoa {names} thành công."
                    : $"Đã khôi phục các Khoa: {names} thành công.";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy Khoa nào để khôi phục.";
            }

            return RedirectToAction("Trash");
        }
    }
}

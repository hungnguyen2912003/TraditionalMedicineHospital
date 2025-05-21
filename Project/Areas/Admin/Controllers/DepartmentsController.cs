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
    [Route("khoa")]
    public class DepartmentsController : Controller
    {
        private readonly IDepartmentRepository _repository;
        private readonly IRoomRepository _roomRepository;
        private readonly IMapper _mapper;
        private readonly CodeGeneratorHelper _codeGenerator;
        public DepartmentsController
        (
            IDepartmentRepository repository,
            IRoomRepository roomRepository,
            ITreatmentMethodRepository treatmentRepository,
            IMapper mapper,
            CodeGeneratorHelper codeGenerator
        )
        {
            _repository = repository;
            _roomRepository = roomRepository;
            _mapper = mapper;
            _codeGenerator = codeGenerator;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _repository.GetAllAsync();
            return View(list);
        }

        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return NotFound();
            return View(entity);
        }

        [HttpGet("them-moi")]
        public async Task<IActionResult> Create()
        {
            var model = new DepartmentDto
            {
                Code = await _codeGenerator.GenerateUniqueCodeAsync(_repository)
            };
            return View(model);
        }

        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] DepartmentDto inputDto)
        {
            try
            {
                var entity = _mapper.Map<Department>(inputDto);

                entity.CreatedBy = "Admin";
                entity.CreatedDate = DateTime.UtcNow;

                await _repository.CreateAsync(entity);
                return Json(new { success = true, message = "Thêm khoa thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm khoa: " + ex.Message });
            }
        }

        [HttpGet("chinh-sua/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<DepartmentDto>(entity);

            ViewBag.DepartmentId = entity.Id;

            return View(dto);
        }

        [HttpPost("chinh-sua/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] DepartmentDto inputDto, Guid id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
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


        [HttpPost("xoa")]
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
               var r = await _roomRepository.GetAllAdvancedAsync();
               var hasRooms = r.Any(x => x.DepartmentId == id);
               if (hasRooms)
               {
                   departments.Add(dep);
               }
            }

            if (departments.Any())
            {
               var names = string.Join(", ", departments.Select(c => $"\"{c.Name}\""));
               var message = departments.Count == 1
                   ? $"Không thể xóa Khoa {names} vì vẫn còn phòng đang thuộc Khoa này."
                   : $"Không thể xóa các Khoa: {names} vì vẫn còn phòng đang thuộc Khoa này.";
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
                    : $"Đã xóa các Khoa thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy Khoa nào để xóa.";
            }

            return RedirectToAction("Index");
        }
    }
}

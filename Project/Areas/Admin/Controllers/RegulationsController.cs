using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Repositories.Interfaces;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class RegulationsController : Controller
    {
        private readonly IRegulationRepository _repository;
        private readonly IMapper _mapper;

        public RegulationsController
        (
            IRegulationRepository repository,
            IMapper mapper
        )
        {
            _repository = repository;
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
        public async Task<IActionResult> Create([FromForm] RegulationDto inputDto)
        {
            try
            {
                var entity = _mapper.Map<Regulation>(inputDto);

                entity.CreatedBy = "Admin";
                entity.CreatedDate = DateTime.UtcNow;
                entity.IsActive = true;

                if (entity.EffectiveDate == default(DateTime))
                {
                    entity.EffectiveDate = DateTime.UtcNow; // Gán giá trị mặc định nếu cần
                }
                if (entity.ExpirationDate == default(DateTime))
                {
                    entity.ExpirationDate = DateTime.UtcNow.AddDays(30); // Gán giá trị mặc định nếu cần
                }

                await _repository.CreateAsync(entity);
                return Json(new { success = true, message = "Thêm quy định thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm quy định: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<RegulationDto>(entity);

            ViewBag.RegulationId = entity.Id;

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] RegulationDto inputDto, Guid Id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(Id);
                if (entity == null) return NotFound();

                _mapper.Map(inputDto, entity);
                entity.UpdatedBy = "Admin";
                entity.UpdatedDate = DateTime.UtcNow;

                await _repository.UpdateAsync(entity);
                return Json(new { success = true, message = "Cập nhật quy định thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật quy định: " + ex.Message });
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

            var delList = new List<Regulation>();
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
                    ? $"Đã xóa quy định {names} thành công"
                    : $"Đã xóa các quy định: {names} thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy quy định nào để xóa.";
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

            var movedList = new List<Regulation>();
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
                    ? $"Đã đưa quy định {names} thành công vào thùng rác"
                    : $"Đã đưa các quy định: {names} thành công vào thùng rác";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy quy định nào để đưa vào thùng rác.";
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
            var restoredEntity = new List<Regulation>();
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
                    ? $"Đã khôi phục quy định {names} thành công."
                    : $"Đã khôi phục các quy định: {names} thành công.";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy quy định nào để khôi phục.";
            }

            return RedirectToAction("Trash");
        }
    }
}

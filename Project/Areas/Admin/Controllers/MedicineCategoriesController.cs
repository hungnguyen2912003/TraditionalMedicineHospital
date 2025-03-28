using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Repositories.Interfaces;
using Project.Services.Interfaces;
using Project.Validators;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MedicineCategoriesController : Controller
    {
        private readonly IMedicineCategoryRepository _repository;
        private readonly IMapper _mapper;
        private readonly IImageService _service;
        private readonly IValidator<MedicineCategoryDto> _validator;

        public MedicineCategoriesController
        (
            IMedicineCategoryRepository repository, 
            IMapper mapper,
            IImageService service, 
            IValidator<MedicineCategoryDto> validator
        )
        {
            _repository = repository;
            _mapper = mapper;
            _service = service;
            _validator = validator;
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
        public async Task<IActionResult> Create([FromForm] MedicineCategoryDto inputDto)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(inputDto);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToList();
                    return Json(new { success = false, message = "Thêm loại thuốc thất bại. Vui lòng kiểm tra lại thông tin.", errors });
                }

                var entity = _mapper.Map<MedicineCategory>(inputDto);

                entity.CreatedBy = "Admin";
                entity.CreatedDate = DateTime.UtcNow;
                entity.IsActive = true;

                if (inputDto.ImageFile != null && inputDto.ImageFile.Length > 0)
                {
                    entity.Images = await _service.SaveImageAsync(inputDto.ImageFile, "MedicineCategories");
                }
                await _repository.CreateAsync(entity);
                return Json(new { success = true, message = "Thêm loại thuốc thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm loại thuốc: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<MedicineCategoryDto>(entity);

            ViewBag.MedicineCategoryId = entity.Id;
            ViewBag.ExistingImage = entity.Images;

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] MedicineCategoryDto inputDto, Guid Id)
        {
            try
            {
                var validator = new MedicineCategoryValidator(_repository, Id);
                var validationResult = await validator.ValidateAsync(inputDto);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => $"{e.ErrorMessage}").ToList();
                    ViewBag.MedicineCategoryId = Id;
                    ViewBag.ExistingImage = (await _repository.GetByIdAsync(Id))?.Images;
                    return Json(new { success = false, message = "Cập nhật loại thuốc thất bại. Vui lòng kiểm tra lại thông tin.", errors });
                }
                var entity = await _repository.GetByIdAsync(Id);
                if (entity == null)
                {
                    return NotFound();
                }
                entity.Code = inputDto.Code;
                entity.Name = inputDto.Name;
                entity.Description = inputDto.Description;
                entity.UpdatedBy = "Admin";
                entity.UpdatedDate = DateTime.UtcNow;

                if (inputDto.ImageFile != null && inputDto.ImageFile.Length > 0)
                {
                    entity.Images = await _service.SaveImageAsync(inputDto.ImageFile, "MedicineCategories");
                }

                await _repository.UpdateAsync(entity);
                return Json(new { success = true, message = "Cập nhật loại thuốc thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật loại thuốc: " + ex.Message });
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

            var deletedCategories = new List<MedicineCategory>();
            foreach (var id in ids)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    if (!string.IsNullOrEmpty(entity.Images))
                    {
                        _service.DeleteImage(entity.Images, "MedicineCategories");
                    }
                    await _repository.DeleteAsync(id);
                    deletedCategories.Add(entity);
                }
            }

            if (deletedCategories.Any())
            {
                var names = string.Join(", ", deletedCategories.Select(c => $"\"{c.Name}\""));
                var message = deletedCategories.Count == 1
                    ? $"Đã xóa vĩnh viễn loại thuốc {names} thành công"
                    : $"Đã xóa vĩnh viễn các loại thuốc {names} thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy loại thuốc nào để xóa.";
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

            var movedCategories = new List<MedicineCategory>();
            foreach (var id in ids)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    entity.IsActive = false;
                    entity.UpdatedBy = "Admin";
                    entity.UpdatedDate = DateTime.UtcNow;
                    await _repository.UpdateAsync(entity);
                    movedCategories.Add(entity);
                }
            }

            if (movedCategories.Any())
            {
                var names = string.Join(", ", movedCategories.Select(c => $"\"{c.Name}\""));
                var message = movedCategories.Count == 1
                    ? $"Đã đưa loại thuốc {names} thành công vào thùng rác"
                    : $"Đã đưa các loại thuốc {names} thành công vào thùng rác";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy loại thuốc nào để đưa vào thùng rác.";
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
            var movedCategories = new List<MedicineCategory>();
            foreach (var id in ids)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    entity.IsActive = true;
                    entity.UpdatedBy = "Admin";
                    entity.UpdatedDate = DateTime.UtcNow;
                    await _repository.UpdateAsync(entity);
                    movedCategories.Add(entity);
                }
            }

            if (movedCategories.Any())
            {
                var names = string.Join(", ", movedCategories.Select(c => $"\"{c.Name}\""));
                var message = movedCategories.Count == 1
                    ? $"Đã khôi phục loại thuốc {names} thành công."
                    : $"Đã khôi phục các loại thuốc {names} thành công.";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy loại thuốc nào để khôi phục.";
            }

            return RedirectToAction("Trash");
        }
    }
}

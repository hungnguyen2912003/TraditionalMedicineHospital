using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.Admin.Models.Enums;
using Project.Extensions;
using Project.Repositories.Interfaces;
using Project.Services.Interfaces;
using Project.Validators;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MedicinesController : Controller
    {
        private readonly IMedicineRepository _repository;
        private readonly IMedicineCategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IImageService _service;
        private readonly IValidator<MedicineDto> _validator;
        public MedicinesController
        (
            IMedicineRepository repository,
            IMapper mapper,
            IImageService service,
            IValidator<MedicineDto> validator,
            IMedicineCategoryRepository categoryRepository
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
            var dtoList = _mapper.Map<List<MedicineDto>>(activeList);
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
            var medicineCategories = await _categoryRepository.GetAllAsync();
            ViewBag.MedicineCategories = medicineCategories
                .Where(mc => mc.IsActive)
                .Select(mc => new { mc.Id, mc.Name })
                .ToList();

            ViewBag.StockUnit = Enum.GetValues(typeof(UnitType))
                .Cast<UnitType>()
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
        public async Task<IActionResult> Create([FromForm] MedicineDto inputDto)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(inputDto);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToList();
                    return Json(new { success = false, message = "Thêm thuốc thất bại. Vui lòng kiểm tra lại thông tin.", errors });
                }

                var entity = _mapper.Map<Medicine>(inputDto);
                entity.CreatedBy = "Admin";
                entity.CreatedDate = DateTime.UtcNow;
                entity.IsActive = true;

                if (inputDto.ImageFile != null && inputDto.ImageFile.Length > 0)
                {
                    entity.Images = await _service.SaveImageAsync(inputDto.ImageFile, "Medicines");
                }

                await _repository.CreateAsync(entity);
                return Json(new { success = true, message = "Thêm thuốc thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm thuốc: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<MedicineDto>(entity);
            ViewBag.MedicineId = entity.Id;
            ViewBag.ExistingImage = entity.Images;


            var medicineCategories = await _categoryRepository.GetAllActiveAsync();
            ViewBag.MedicineCategories = medicineCategories
                .Select(mc => new { mc.Id, mc.Name })
                .ToList();

            ViewBag.StockUnit = Enum.GetValues(typeof(UnitType))
                .Cast<UnitType>()
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
        public async Task<IActionResult> Edit([FromForm] MedicineDto inputDto, Guid Id)
        {
            try
            {
                var validator = new MedicineValidator(_repository, Id);
                var validationResult = await validator.ValidateAsync(inputDto);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => $"{e.ErrorMessage}").ToList();
                    return Json(new { success = false, message = "Cập nhật thuốc thất bại. Vui lòng kiểm tra lại thông tin.", errors });
                }

                var entity = await _repository.GetByIdAsync(Id);
                if (entity == null) return NotFound();
                entity = _mapper.Map(inputDto, entity);
                entity.UpdatedBy = "Admin";
                entity.UpdatedDate = DateTime.UtcNow;
                if (inputDto.ImageFile != null && inputDto.ImageFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(entity.Images))
                    {
                        _service.DeleteImage(entity.Images, "Medicines");
                    }
                    entity.Images = await _service.SaveImageAsync(inputDto.ImageFile, "Medicines");
                }
                await _repository.UpdateAsync(entity);
                return Json(new { success = true, message = "Cập nhật thuốc thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật thuốc: " + ex.Message });
            }
        }

        public async Task<IActionResult> Trash()
        {
            var list = await _repository.GetAllWithCategoryAsync();
            var trashList = list.Where(x => x.IsActive == false).ToList();
            var dtoList = _mapper.Map<List<MedicineDto>>(trashList);
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

            var deletedMedicines = new List<Medicine>();
            foreach (var id in ids)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    if (!string.IsNullOrEmpty(entity.Images))
                    {
                        _service.DeleteImage(entity.Images, "Medicines");
                    }
                    await _repository.DeleteAsync(id);
                    deletedMedicines.Add(entity);
                }
            }

            if (deletedMedicines.Any())
            {
                var names = string.Join(", ", deletedMedicines.Select(c => $"\"{c.Name}\""));
                var message = deletedMedicines.Count == 1
                    ? $"Đã xóa vĩnh viễn thuốc {names} thành công"
                    : $"Đã xóa vĩnh viễn các thuốc {names} thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy thuốc nào để xóa.";
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

            var deletedMedicines = new List<Medicine>();
            foreach (var id in ids)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    entity.IsActive = false;
                    entity.UpdatedBy = "Admin";
                    entity.UpdatedDate = DateTime.UtcNow;
                    await _repository.UpdateAsync(entity);
                    deletedMedicines.Add(entity);
                }
            }

            if (deletedMedicines.Any())
            {
                var names = string.Join(", ", deletedMedicines.Select(c => $"\"{c.Name}\""));
                var message = deletedMedicines.Count == 1
                    ? $"Đã đưa thuốc {names} thành công vào thùng rác"
                    : $"Đã đưa các thuốc {names} thành công vào thùng rác";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy thuốc nào để đưa vào thùng rác.";
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
            var deletedMedicines = new List<Medicine>();
            foreach (var id in ids)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    entity.IsActive = true;
                    entity.UpdatedBy = "Admin";
                    entity.UpdatedDate = DateTime.UtcNow;
                    await _repository.UpdateAsync(entity);
                    deletedMedicines.Add(entity);
                }
            }

            if (deletedMedicines.Any())
            {
                var names = string.Join(", ", deletedMedicines.Select(c => $"\"{c.Name}\""));
                var message = deletedMedicines.Count == 1
                    ? $"Đã khôi phục thuốc {names} thành công."
                    : $"Đã khôi phục các thuốc {names} thành công.";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy thuốc nào để khôi phục.";
            }

            return RedirectToAction("Trash");
        }
    }
}

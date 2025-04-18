﻿using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Helpers;
using Project.Repositories.Interfaces;
using Project.Services.Interfaces;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MedicineCategoriesController : Controller
    {
        private readonly IMedicineCategoryRepository _repository;
        private readonly IMedicineRepository _medicineRepository;
        private readonly IMapper _mapper;
        private readonly IImageService _imgService;
        private readonly CodeGeneratorHelper _codeGenerator;

        public MedicineCategoriesController
        (
            IMedicineCategoryRepository repository,
            IMedicineRepository medicineRepository,
            IMapper mapper,
            IImageService imgService,
            CodeGeneratorHelper codeGenerator
        )
        {
            _repository = repository;
            _medicineRepository = medicineRepository;
            _mapper = mapper;
            _imgService = imgService;
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
            var model = new MedicineCategoryDto
            {
                Code = await _codeGenerator.GenerateUniqueCodeAsync(_repository)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] MedicineCategoryDto inputDto)
        {
            try
            {
                var entity = _mapper.Map<MedicineCategory>(inputDto);

                entity.CreatedBy = "Admin";
                entity.CreatedDate = DateTime.UtcNow;
                entity.IsActive = true;

                if (inputDto.ImageFile != null && inputDto.ImageFile.Length > 0)
                {
                    entity.Images = await _imgService.SaveImageAsync(inputDto.ImageFile, "MedicineCategories");
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
                var entity = await _repository.GetByIdAsync(Id);
                if (entity == null) return NotFound();

                _mapper.Map(inputDto, entity);
                entity.UpdatedBy = "Admin";
                entity.UpdatedDate = DateTime.UtcNow;

                if (inputDto.ImageFile != null && inputDto.ImageFile.Length > 0)
                {
                    entity.Images = await _imgService.SaveImageAsync(inputDto.ImageFile, "MedicineCategories");
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
            foreach (var id in selectedIds.Split(','))
            {
                if (Guid.TryParse(id, out var parsedId))
                {
                    ids.Add(parsedId);
                }
            }

            // Kiểm tra xem có Medicine nào đang sử dụng MedicineCategory không
            var categories = new List<MedicineCategory>();
            foreach (var id in ids)
            {
                var category = await _repository.GetByIdAsync(id);
                if (category == null) continue;
                // Lấy danh sách Medicine có MedicineCategoryId trỏ đến category.Id
                var medicines = await _medicineRepository.GetAllAdvancedAsync();
                var hasMedicines = medicines.Any(m => m.MedicineCategoryId == id);
                if (hasMedicines)
                {
                    categories.Add(category);
                }
            }

            // Nếu có MedicineCategory đang được sử dụng, trả về thông báo lỗi
            if (categories.Any())
            {
                var names = string.Join(", ", categories.Select(c => $"\"{c.Name}\""));
                var message = categories.Count == 1
                    ? $"Không thể xóa loại thuốc {names} vì vẫn còn thuốc đang sử dụng loại này."
                    : $"Không thể xóa các loại thuốc: {names} vì vẫn còn thuốc đang sử dụng các loại này.";
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Index");
            }

            // Nếu không có Medicine nào => Xóa
            var delList = new List<MedicineCategory>();
            foreach (var id in ids)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    if (!string.IsNullOrEmpty(entity.Images))
                    {
                        _imgService.DeleteImage(entity.Images, "MedicineCategories");
                    }
                    await _repository.DeleteAsync(id);
                    delList.Add(entity);
                }
            }

            if (delList.Any())
            {
                var names = string.Join(", ", delList.Select(c => $"\"{c.Name}\""));
                var message = delList.Count == 1
                    ? $"Đã xóa loại thuốc {names} thành công"
                    : $"Đã xóa các loại thuốc {names} thành công";
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

            // Kiểm tra xem có Medicine nào đang sử dụng MedicineCategory không
            var categories = new List<MedicineCategory>();
            foreach (var id in ids)
            {
                var category = await _repository.GetByIdAsync(id);
                if (category == null) continue;

                // Lấy danh sách Medicine có MedicineCategoryId trỏ đến category.Id
                var medicines = await _medicineRepository.GetAllAdvancedAsync();
                var hasMedicines = medicines.Any(m => m.MedicineCategoryId == id);

                if (hasMedicines)
                {
                    categories.Add(category);
                }
            }

            // Nếu có MedicineCategory đang được sử dụng, trả về thông báo lỗi
            if (categories.Any())
            {
                var names = string.Join(", ", categories.Select(c => $"\"{c.Name}\""));
                var message = categories.Count == 1
                    ? $"Không thể đưa loại thuốc {names} vào thùng rác vì vẫn còn thuốc đang sử dụng loại này."
                    : $"Không thể đưa các loại thuốc: {names} vào thùng rác vì vẫn còn thuốc đang sử dụng các loại này.";
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Index");
            }

            // Nếu không có Medicine nào => Chuyển vào thùng rác
            var movedList = new List<MedicineCategory>();
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
            var restoredList = new List<MedicineCategory>();
            foreach (var id in ids)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    entity.IsActive = true;
                    entity.UpdatedBy = "Admin";
                    entity.UpdatedDate = DateTime.UtcNow;
                    await _repository.UpdateAsync(entity);
                    restoredList.Add(entity);
                }
            }

            if (restoredList.Any())
            {
                var names = string.Join(", ", restoredList.Select(c => $"\"{c.Name}\""));
                var message = restoredList.Count == 1
                    ? $"Đã khôi phục loại thuốc {names} thành công."
                    : $"Đã khôi phục các loại thuốc: {names} thành công.";
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

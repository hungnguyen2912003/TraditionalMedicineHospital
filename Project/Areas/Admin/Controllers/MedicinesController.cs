using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.Admin.Models.ViewModels;
using Project.Helpers;
using Project.Repositories.Interfaces;
using Project.Services.Interfaces;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MedicinesController : Controller
    {
        private readonly IMedicineRepository _repository;
        private readonly IMedicineCategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IImageService _imgService;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly CodeGeneratorHelper _codeGenerator;

        public MedicinesController
        (
            IMedicineRepository repository,
            IMapper mapper,
            IImageService imgService,
            IMedicineCategoryRepository categoryRepository,
            ViewBagHelper viewBagHelper,
            CodeGeneratorHelper codeGenerator
        )
        {
            _repository = repository;
            _mapper = mapper;
            _imgService = imgService;
            _categoryRepository = categoryRepository;
            _viewBagHelper = viewBagHelper;
            _codeGenerator = codeGenerator;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _repository.GetAllAdvancedAsync();
            var activeList = list.Where(x => x.IsActive == true).ToList();
            var viewModelList = _mapper.Map<List<MedicineViewModel>>(activeList);
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(viewModelList);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var entity = await _repository.GetByIdAdvancedAsync(id);
            if (entity == null) return NotFound();
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(entity);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await _viewBagHelper.BaseViewBag(ViewData);
            var model = new MedicineDto
            {
                Code = await _codeGenerator.GenerateUniqueCodeAsync(_repository)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] MedicineDto inputDto)
        {
            try
            {
                var entity = _mapper.Map<Medicine>(inputDto);
                entity.CreatedBy = "Admin";
                entity.CreatedDate = DateTime.UtcNow;
                entity.IsActive = true;

                if (inputDto.ImageFile != null && inputDto.ImageFile.Length > 0)
                {
                    entity.Images = await _imgService.SaveImageAsync(inputDto.ImageFile, "Medicines");
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
            var entity = await _repository.GetByIdAdvancedAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<MedicineDto>(entity);

            ViewBag.MedicineId = entity.Id;
            ViewBag.ExistingImage = entity.Images;

            await _viewBagHelper.BaseViewBag(ViewData);

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] MedicineDto inputDto, Guid Id)
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
                    entity.Images = await _imgService.SaveImageAsync(inputDto.ImageFile, "Medicines");
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
            var list = await _repository.GetAllAdvancedAsync();
            var activeList = list.Where(x => x.IsActive == false).ToList();
            var viewModelList = _mapper.Map<List<MedicineViewModel>>(activeList);
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(viewModelList);
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

            var delList = new List<Medicine>();
            foreach (var id in ids)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    if (!string.IsNullOrEmpty(entity.Images))
                    {
                        _imgService.DeleteImage(entity.Images, "Medicines");
                    }
                    await _repository.DeleteAsync(id);
                    delList.Add(entity);
                }
            }

            if (delList.Any())
            {
                var names = string.Join(", ", delList.Select(c => $"\"{c.Name}\""));
                var message = delList.Count == 1
                    ? $"Đã xóa vĩnh viễn thuốc {names} thành công"
                    : $"Đã xóa vĩnh viễn các thuốc: {names} thành công";
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

            var movedList = new List<Medicine>();
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
                    ? $"Đã đưa thuốc {names} thành công vào thùng rác"
                    : $"Đã đưa các thuốc: {names} thành công vào thùng rác";
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
            var restoredList = new List<Medicine>();
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
                    ? $"Đã khôi phục thuốc {names} thành công."
                    : $"Đã khôi phục các thuốc: {names} thành công.";
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

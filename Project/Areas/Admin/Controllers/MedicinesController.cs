using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.Admin.Models.ViewModels;
using Project.Helpers;
using Project.Repositories.Interfaces;
using Project.Services.Interfaces;
using Repositories.Interfaces;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("thuoc")]
    public class MedicinesController : Controller
    {
        private readonly IMedicineRepository _repository;
        private readonly IMedicineCategoryRepository _categoryRepository;
        private readonly IPrescriptionDetailRepository _prescriptionDetailRepository;
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
            IPrescriptionDetailRepository prescriptionDetailRepository,
            ViewBagHelper viewBagHelper,
            CodeGeneratorHelper codeGenerator
        )
        {
            _repository = repository;
            _mapper = mapper;
            _imgService = imgService;
            _categoryRepository = categoryRepository;
            _prescriptionDetailRepository = prescriptionDetailRepository;
            _viewBagHelper = viewBagHelper;
            _codeGenerator = codeGenerator;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _repository.GetAllAdvancedAsync();
            var viewModelList = _mapper.Map<List<MedicineViewModel>>(list);
            viewModelList = viewModelList.OrderBy(x => x.CategoryName).ToList();
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(viewModelList);
        }

        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var entity = await _repository.GetByIdAdvancedAsync(id);
            if (entity == null) return NotFound();
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(entity);
        }

        [HttpGet("them-moi")]
        public async Task<IActionResult> Create()
        {
            await _viewBagHelper.BaseViewBag(ViewData);
            var model = new MedicineDto
            {
                Code = await _codeGenerator.GenerateUniqueCodeAsync(_repository)
            };
            return View(model);
        }

        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] MedicineDto inputDto)
        {
            try
            {
                var entity = _mapper.Map<Medicine>(inputDto);
                entity.CreatedBy = "Admin";
                entity.CreatedDate = DateTime.UtcNow;

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

        [HttpGet("chinh-sua/{id}")]
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

        [HttpPost("chinh-sua/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] MedicineDto inputDto, Guid id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
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

            // Lấy tất cả PrescriptionDetail có MedicineId thuộc ids
            var allPrescriptionDetails = await _prescriptionDetailRepository.GetAllAsync();
            var usedPrescriptionDetails = allPrescriptionDetails.Where(d => ids.Contains(d.MedicineId)).ToList();

            if (usedPrescriptionDetails.Any())
            {
                var names = string.Join(", ", usedPrescriptionDetails.Select(d => $"\"{d.Medicine?.Name}\"").Distinct());
                var message = usedPrescriptionDetails.Count == 1
                    ? $"Không thể xóa thuốc {names} vì vẫn còn đơn thuốc đang sử dụng."
                    : $"Không thể xóa các thuốc này vì vẫn còn đơn thuốc đang sử dụng.";
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Index");
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
                    : $"Đã xóa vĩnh viễn các thuốc thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy thuốc nào để xóa.";
            }

            return RedirectToAction("Index");
        }
    }
}

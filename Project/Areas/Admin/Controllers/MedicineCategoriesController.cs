using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Repositories.Interfaces;
using Project.Validators;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MedicineCategoriesController : Controller
    {
        private readonly IMedicineCategoryRepository _repository;
        private readonly IMapper _mapper;
        private readonly ImageValidator _imageValidator;
        private readonly IValidator<MedicineCategoryDto> _validator;

        public MedicineCategoriesController
        (
            IMedicineCategoryRepository repository, 
            IMapper mapper, 
            ImageValidator imageValidator, 
            IValidator<MedicineCategoryDto> validator,
            INotyfService notify
        )
        {
            _repository = repository;
            _mapper = mapper;
            _imageValidator = imageValidator;
            _validator = validator;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _repository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<MedicineCategoryDto>>(list);
            return View(dtos);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return NotFound();
            var dto = _mapper.Map<MedicineCategoryDto>(entity);
            return View(dto);
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
            var validationResult = await _validator.ValidateAsync(inputDto);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return View(inputDto);
            }
            var entity = _mapper.Map<MedicineCategory>(inputDto);
            entity.CreatedBy = "Admin";
            entity.CreatedDate = DateTime.UtcNow;
            entity.Status = true;

            if (inputDto.ImageFile != null && inputDto.ImageFile.Length > 0)
            {
                entity.Images = await _imageValidator.SaveImageAsync(inputDto.ImageFile, "MedicineCategories");
            }

            await _repository.CreateAsync(entity);
            TempData["SuccessMessage"] = "Thêm loại thuốc thành công!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<MedicineCategoryDto>(entity);
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] MedicineCategoryDto inputDto)
        {
            var validationResult = await _validator.ValidateAsync(inputDto);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return View(inputDto);
            }

            var entity = _mapper.Map<MedicineCategory>(inputDto);
            entity.UpdatedBy = "Admin";
            entity.UpdatedDate = DateTime.UtcNow;
            await _repository.UpdateAsync(entity);
            TempData["SuccessMessage"] = "Cập nhật loại thuốc thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _repository.DeleteAsync(id);
            TempData["SuccessMessage"] = "Xóa loại thuốc thành công!";
            return RedirectToAction("Index");
        }
    }
}

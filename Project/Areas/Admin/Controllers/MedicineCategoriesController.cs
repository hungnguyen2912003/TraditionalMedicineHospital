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
            IValidator<MedicineCategoryDto> validator
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
            return View(list);
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
            return RedirectToAction("Index");
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
        public async Task<IActionResult> Edit([FromForm] MedicineCategoryDto inputDto, Guid Id, [FromForm] bool IsImageRemoved)
        {
            var validationResult = await _validator.ValidateAsync(inputDto);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                ViewBag.MedicineCategoryId = Id;
                ViewBag.ExistingImage = (await _repository.GetByIdAsync(Id))?.Images;
                return View(inputDto);
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

            if (IsImageRemoved && inputDto.ImageFile == null)
            {
                if (!string.IsNullOrEmpty(entity.Images))
                {
                    _imageValidator.DeleteImage(entity.Images, "MedicineCategories");
                    entity.Images = null;
                }
            }
            else if (inputDto.ImageFile != null && inputDto.ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(entity.Images))
                {
                    _imageValidator.DeleteImage(entity.Images, "MedicineCategories");
                }
                entity.Images = await _imageValidator.SaveImageAsync(inputDto.ImageFile, "MedicineCategories");
            }
            await _repository.UpdateAsync(entity);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Trash()
        {
            var list = await _repository.GetAllAsync();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _repository.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}

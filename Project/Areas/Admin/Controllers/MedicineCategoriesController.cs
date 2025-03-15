using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Repositories.Interfaces;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MedicineCategoriesController : Controller
    {
        private readonly IMedicineCategoryRepository _repository;
        private readonly IMapper _mapper;

        public MedicineCategoriesController(IMedicineCategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
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
            return View(new MedicineCategoryDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] MedicineCategoryDto inputDto)
        {
            if (ModelState.IsValid)
            {
                var entity = _mapper.Map<MedicineCategory>(inputDto);
                entity.CreatedBy = "Admin";
                entity.CreatedDate = DateTime.UtcNow;
                entity.Status = true;

                await _repository.CreateAsync(entity);
                return RedirectToAction("Index");
            }
            return View(inputDto);
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
            if (ModelState.IsValid)
            {
                var entity = _mapper.Map<MedicineCategory>(inputDto);
                entity.UpdatedBy = "Admin";
                entity.UpdatedDate = DateTime.UtcNow;
                await _repository.UpdateAsync(entity);
                return RedirectToAction("Index");
            }
            return View(inputDto);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _repository.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}

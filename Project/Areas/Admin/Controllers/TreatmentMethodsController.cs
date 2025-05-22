using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.Admin.Models.ViewModels;
using Project.Helpers;
using Project.Repositories.Interfaces;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("phuong-phap-dieu-tri")]
    public class TreatmentMethodsController : Controller
    {
        private readonly ITreatmentMethodRepository _repository;
        private readonly IRoomRepository _roomRepository;
        private readonly IMapper _mapper;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly CodeGeneratorHelper _codeGenerator;

        public TreatmentMethodsController
        (
            ITreatmentMethodRepository repository,
            IRoomRepository roomRepository,
            IMapper mapper,
            ViewBagHelper viewBagHelper,
            CodeGeneratorHelper codeGenerator
        )
        {
            _repository = repository;
            _roomRepository = roomRepository;
            _mapper = mapper;
            _viewBagHelper = viewBagHelper;
            _codeGenerator = codeGenerator;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _repository.GetAllAdvancedAsync();
            var viewModelList = _mapper.Map<List<TreatmentMethodViewModel>>(list);
            return View(viewModelList);
        }

        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var entity = await _repository.GetByIdAdvancedAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            return View(entity);
        }

        [HttpGet("them-moi")]
        public async Task<IActionResult> Create()
        {
            await _viewBagHelper.BaseViewBag(ViewData);

            var model = new TreatmentMethodDto
            {
                Code = await _codeGenerator.GenerateUniqueCodeAsync(_repository)
            };

            return View(model);
        }

        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] TreatmentMethodDto inputDto)
        {
            try
            {
                var entity = _mapper.Map<TreatmentMethod>(inputDto);

                entity.DepartmentId = inputDto.DepartmentId;
                entity.CreatedBy = "Admin";
                entity.CreatedDate = DateTime.UtcNow;

                await _repository.CreateAsync(entity);
                return Json(new { success = true, message = "Thêm phương pháp điều trị thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm phương pháp điều trị: " + ex.Message });
            }
        }

        [HttpGet("chinh-sua/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var entity = await _repository.GetByIdAdvancedAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<TreatmentMethodDto>(entity);

            ViewBag.TreatmentMethodId = entity.Id;

            await _viewBagHelper.BaseViewBag(ViewData);

            return View(dto);
        }

        [HttpPost("chinh-sua/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] TreatmentMethodDto inputDto, Guid Id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(Id);
                if (entity == null) return NotFound();

                _mapper.Map(inputDto, entity);
                entity.DepartmentId = inputDto.DepartmentId;
                entity.UpdatedBy = "Admin";
                entity.UpdatedDate = DateTime.UtcNow;

                await _repository.UpdateAsync(entity);
                return Json(new { success = true, message = "Cập nhật phương pháp điều trị thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật phương pháp điều trị: " + ex });
            }
        }

        [HttpPost("xoa")]
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

            var treatments = new List<TreatmentMethod>();
            foreach (var id in ids)
            {
                var tm = await _repository.GetByIdAsync(id);
                if (tm == null) continue;
                var r = await _roomRepository.GetAllAdvancedAsync();
                var hasRooms = r.Any(m => m.TreatmentMethodId == id);
                if (hasRooms)
                {
                    treatments.Add(tm);
                }
            }

            if (treatments.Any())
            {
                var names = string.Join(", ", treatments.Select(c => $"\"{c.Name}\""));
                var message = treatments.Count == 1
                    ? $"Không thể xóa phương pháp điều trị {names} vì vẫn còn phòng đang sử dụng phương pháp này."
                    : $"Không thể xóa các phương pháp điều trị: {names} vì vẫn còn các phòng đang sử dụng phương pháp này.";
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Index");
            }

            var delList = new List<TreatmentMethod>();
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
                    ? $"Đã xóa vĩnh viễn phương pháp điều trị {names} thành công"
                    : $"Đã xóa vĩnh viễn các phương pháp điều trị thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy phương pháp điều trị nào để xóa.";
            }

            return RedirectToAction("Index");
        }
    }
}

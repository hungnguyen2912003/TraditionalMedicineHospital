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
    [Route("phong")]
    public class RoomsController : Controller
    {
        private readonly IRoomRepository _repository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ITreatmentRecordDetailRepository _treatmentRecordDetailRepository;
        private readonly IMapper _mapper;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly CodeGeneratorHelper _codeGenerator;

        public RoomsController
        (
            IRoomRepository repository,
            IEmployeeRepository employeeRepository,
            ITreatmentRecordDetailRepository treatmentRecordDetailRepository,
            IMapper mapper,
            ViewBagHelper viewBagHelper,
            CodeGeneratorHelper codeGenerator
        )
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
            _treatmentRecordDetailRepository = treatmentRecordDetailRepository;
            _mapper = mapper;
            _viewBagHelper = viewBagHelper;
            _codeGenerator = codeGenerator;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _repository.GetAllAdvancedAsync();
            var viewModelList = _mapper.Map<List<RoomViewModel>>(list);
            viewModelList = viewModelList.OrderBy(x => x.DepartmentName).ToList();
            await _viewBagHelper.BaseViewBag(ViewData);
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

            var model = new RoomDto
            {
                Code = await _codeGenerator.GenerateUniqueCodeAsync(_repository)
            };

            return View(model);
        }

        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] RoomDto inputDto)
        {
            try
            {
                var entity = _mapper.Map<Room>(inputDto);
                entity.CreatedBy = "Admin";
                entity.CreatedDate = DateTime.UtcNow;

                await _repository.CreateAsync(entity);
                return Json(new { success = true, message = "Thêm phòng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm phòng: " + ex.Message });
            }
        }

        [HttpGet("chinh-sua/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var entity = await _repository.GetByIdAdvancedAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<RoomDto>(entity);

            ViewBag.RoomId = entity.Id;

            await _viewBagHelper.BaseViewBag(ViewData);

            return View(dto);
        }

        [HttpPost("chinh-sua/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] RoomDto inputDto, Guid Id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(Id);
                if (entity == null) return NotFound();

                _mapper.Map(inputDto, entity);
                entity.UpdatedBy = "Admin";
                entity.UpdatedDate = DateTime.UtcNow;

                await _repository.UpdateAsync(entity);
                return Json(new { success = true, message = "Cập nhật phòng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật phòng: " + ex });
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

            // Lấy tất cả Employee có RoomId thuộc ids
            var allEmployees = await _employeeRepository.GetAllAsync();
            var usedEmployees = allEmployees.Where(e => ids.Contains(e.RoomId)).ToList();
            if (usedEmployees.Any())
            {
                var names = string.Join(", ", usedEmployees.Select(e => $"\"{e.Name}\"").Distinct());
                var message = usedEmployees.Count == 1
                    ? $"Không thể xóa phòng {names} vì vẫn còn nhân sự đang làm việc trong phòng này."
                    : $"Không thể xóa các phòng đã chọn vì vẫn còn nhân sự đang làm việc trong các phòng này.";
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Index");
            }

            // Lất tất cả TreatmentRecordDetail có RoomId thuộc ids
            var allTreatmentRecordDetails = await _treatmentRecordDetailRepository.GetAllAsync();
            var usedTreatmentRecordDetails = allTreatmentRecordDetails.Where(t => ids.Contains(t.RoomId)).ToList();
            if (usedTreatmentRecordDetails.Any())
            {
                var names = string.Join(", ", usedTreatmentRecordDetails.Select(t => $"\"{t.Room.Name}\"").Distinct());
                var message = usedTreatmentRecordDetails.Count == 1
                    ? $"Không thể xóa phòng {names} vì vẫn còn chi tiết phiếu khám đang thuộc phòng này."
                    : $"Không thể xóa các phòng đã chọn vì vẫn còn chi tiết phiếu khám đang thuộc các phòng này.";
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Index");
            }

            var delList = new List<Room>();
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
                    ? $"Đã xóa vĩnh viễn phòng {names} thành công"
                    : $"Đã xóa vĩnh viễn các phòng: {names} thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy phòng nào để xóa.";
            }

            return RedirectToAction("Index");
        }
    }
}

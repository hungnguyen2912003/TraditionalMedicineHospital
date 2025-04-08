using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Staff.Models.DTOs;
using Project.Areas.Staff.Models.Entities;
using Project.Areas.Staff.Models.ViewModels;
using Project.Helpers;
using Project.Repositories.Interfaces;

namespace Project.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Admin, Nhanvien")]
    public class TreatmentRecordsController : Controller
    {
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;
        private readonly IMapper _mapper;
        private readonly ViewBagHelper _viewBagHelper;
        public TreatmentRecordsController
        (
            ITreatmentRecordRepository treatmentRecordRepository,
            IMapper mapper,
            ViewBagHelper viewBagHelper
        )
        {
            _treatmentRecordRepository = treatmentRecordRepository;
            _mapper = mapper;
            _viewBagHelper = viewBagHelper;
        }
        public async Task<IActionResult> Index()
        {
            var list = await _treatmentRecordRepository.GetAllAdvancedAsync();
            var activeList = list.Where(x => x.IsActive == true).ToList();
            var viewModelList = _mapper.Map<List<TreatmentRecordViewModel>>(activeList);
            return View(viewModelList);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var treatmentRecord = await _treatmentRecordRepository.GetByIdAsync(id);
            if (treatmentRecord == null)
            {
                return NotFound();
            }
            return View(treatmentRecord);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await _viewBagHelper.BaseViewBag(ViewData);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] TreatmentRecordDto inputDto)
        {
            try
            {
                var entity = _mapper.Map<TreatmentRecord>(inputDto);

                entity.CreatedBy = "Admin";
                entity.CreatedDate = DateTime.UtcNow;
                entity.IsActive = true;
                entity.Status = Project.Models.Enums.TreatmentStatus.DangDieuTri;

                await _treatmentRecordRepository.CreateAsync(entity);

                return Json(new { success = true, message = "Thêm đợt điều trị thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm đợt điều trị: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var entity = await _treatmentRecordRepository.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<TreatmentRecordDto>(entity);

            ViewBag.TreatmentRecordId = entity.Id;

            await _viewBagHelper.BaseViewBag(ViewData);

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] TreatmentRecordDto inputDto, Guid Id)
        {
            try
            {
                var entity = await _treatmentRecordRepository.GetByIdAsync(Id);
                if (entity == null) return NotFound();

                _mapper.Map(inputDto, entity);
                entity.UpdatedBy = "Admin";
                entity.UpdatedDate = DateTime.UtcNow;

                await _treatmentRecordRepository.UpdateAsync(entity);
                return Json(new { success = true, message = "Cập nhật thông tin đợt điều trị thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật đợt điều trị: " + ex.Message });
            }
        }

        public async Task<IActionResult> Trash()
        {
            var list = await _treatmentRecordRepository.GetAllAdvancedAsync();
            var activeList = list.Where(x => x.IsActive == false).ToList();
            var viewModelList = _mapper.Map<List<TreatmentRecordViewModel>>(activeList);
            return View(viewModelList);
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

            var delList = new List<TreatmentRecord>();
            foreach (var id in ids)
            {
                var entity = await _treatmentRecordRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    await _treatmentRecordRepository.DeleteAsync(id);
                    delList.Add(entity);
                }
            }

            if (delList.Any())
            {
                var codes = string.Join(", ", delList.Select(c => $"\"{c.Code}\""));
                var message = delList.Count == 1
                    ? $"Đã xóa đợt điều trị {codes} thành công"
                    : $"Đã xóa các đợt điều trị: {codes} thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy đợt điều trị nào để xóa.";
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

            var movedList = new List<TreatmentRecord>();
            foreach (var id in ids)
            {

                var entity = await _treatmentRecordRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    entity.IsActive = false;
                    entity.UpdatedBy = "Admin";
                    entity.UpdatedDate = DateTime.UtcNow;
                    await _treatmentRecordRepository.UpdateAsync(entity);
                    movedList.Add(entity);
                }
            }

            if (movedList.Any())
            {
                var codes = string.Join(", ", movedList.Select(c => $"\"{c.Code}\""));
                var message = movedList.Count == 1
                    ? $"Đã đưa đợt điều trị {codes} thành công vào thùng rác"
                    : $"Đã đưa các đợt điều trị: {codes} thành công vào thùng rác";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy đợt điều trị nào để đưa vào thùng rác.";
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
            var restoredEntity = new List<TreatmentRecord>();
            foreach (var id in ids)
            {
                var entity = await _treatmentRecordRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    entity.IsActive = true;
                    entity.UpdatedBy = "Admin";
                    entity.UpdatedDate = DateTime.UtcNow;
                    await _treatmentRecordRepository.UpdateAsync(entity);
                    restoredEntity.Add(entity);
                }
            }

            if (restoredEntity.Any())
            {
                var codes = string.Join(", ", restoredEntity.Select(c => $"\"{c.Code}\""));
                var message = restoredEntity.Count == 1
                    ? $"Đã khôi phục đợt điều trị {codes} thành công."
                    : $"Đã khôi phục các đợt điều trị: {codes} thành công.";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy đợt điều trị nào để khôi phục.";
            }

            return RedirectToAction("Trash");
        }
    }
}

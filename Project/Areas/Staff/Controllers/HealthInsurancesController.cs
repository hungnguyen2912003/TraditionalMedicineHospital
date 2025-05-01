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
    public class HealthInsurancesController : Controller
    {
        private readonly IHealthInsuranceRepository _healthInsuranceRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IMapper _mapper;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly CodeGeneratorHelper _codeGenerator;

        public HealthInsurancesController
        (
            IHealthInsuranceRepository healthInsuranceRepository,
            IPatientRepository patientRepository,
            IMapper mapper,
            ViewBagHelper viewBagHelper,
            CodeGeneratorHelper codeGenerator
        )
        {
            _healthInsuranceRepository = healthInsuranceRepository;
            _patientRepository = patientRepository;
            _mapper = mapper;
            _viewBagHelper = viewBagHelper;
            _codeGenerator = codeGenerator;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _healthInsuranceRepository.GetAllAdvancedAsync();
            var activeList = list.Where(x => x.IsActive == true).ToList();
            var viewModelList = _mapper.Map<List<HealthInsuranceViewModel>>(activeList);
            return View(viewModelList);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var healthInsurance = await _healthInsuranceRepository.GetByIdAdvancedAsync(id);
            if (healthInsurance == null)
            {
                return NotFound();
            }
            return View(healthInsurance);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await _viewBagHelper.GetPatientsWithoutInsurance(ViewData);
            var model = new HealthInsuranceDto
            {
                Code = await _codeGenerator.GenerateUniqueCodeAsync(_healthInsuranceRepository)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] HealthInsuranceDto inputDto)
        {
            try
            {
                // Validate that the patient exists
                var patient = await _patientRepository.GetByIdAsync(inputDto.PatientId);
                if (patient == null)
                {
                    return Json(new { success = false, message = "Bệnh nhân không tồn tại trong hệ thống." });
                }

                var entity = _mapper.Map<HealthInsurance>(inputDto);

                entity.CreatedBy = "Admin";
                entity.CreatedDate = DateTime.UtcNow;
                entity.IsActive = true;
                entity.PatientId = inputDto.PatientId; // Explicitly set PatientId

                await _healthInsuranceRepository.CreateAsync(entity);

                return Json(new { success = true, message = "Thêm thẻ BHYT thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm thẻ BHYT: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var entity = await _healthInsuranceRepository.GetByIdAdvancedAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<HealthInsuranceDto>(entity);

            ViewBag.HealthInsuranceId = entity.Id;

            await _viewBagHelper.BaseViewBag(ViewData);

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] HealthInsuranceDto inputDto, Guid Id)
        {
            try
            {
                var entity = await _healthInsuranceRepository.GetByIdAdvancedAsync(Id);
                if (entity == null) return NotFound();

                // Log current state
                var originalPatientId = entity.PatientId;

                // Map the DTO to entity but exclude PatientId
                _mapper.Map(inputDto, entity);

                // Ensure PatientId is preserved
                entity.PatientId = originalPatientId;

                entity.UpdatedBy = "Admin";
                entity.UpdatedDate = DateTime.UtcNow;

                // Validate that the patient still exists
                var patient = await _patientRepository.GetByIdAsync(originalPatientId);
                if (patient == null)
                {
                    return Json(new { success = false, message = "Bệnh nhân không tồn tại trong hệ thống." });
                }

                try
                {
                    await _healthInsuranceRepository.UpdateAsync(entity);
                    return Json(new { success = true, message = "Cập nhật thông tin thẻ BHYT thành công!" });
                }
                catch (Exception updateEx)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Lỗi khi cập nhật thẻ BHYT",
                        details = updateEx.Message,
                        innerException = updateEx.InnerException?.Message,
                        stackTrace = updateEx.StackTrace
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi cập nhật thẻ BHYT",
                    details = ex.Message,
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        public async Task<IActionResult> Trash()
        {
            var list = await _healthInsuranceRepository.GetAllAdvancedAsync();
            var activeList = list.Where(x => x.IsActive == false).ToList();
            var viewModelList = _mapper.Map<List<HealthInsuranceViewModel>>(activeList);
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

            var delList = new List<HealthInsurance>();
            foreach (var id in ids)
            {
                var entity = await _healthInsuranceRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    await _healthInsuranceRepository.DeleteAsync(id);
                    delList.Add(entity);
                }
            }

            if (delList.Any())
            {
                var names = string.Join(", ", delList.Select(c => $"\"{c.Number}\""));
                var message = delList.Count == 1
                    ? $"Đã xóa thẻ BHYT {names} thành công"
                    : $"Đã xóa các thẻ BHYT: {names} thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy thẻ BHYT nào để xóa.";
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

            var movedList = new List<HealthInsurance>();
            foreach (var id in ids)
            {
                var entity = await _healthInsuranceRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    entity.IsActive = false;
                    entity.UpdatedBy = "Admin";
                    entity.UpdatedDate = DateTime.UtcNow;
                    await _healthInsuranceRepository.UpdateAsync(entity);
                    movedList.Add(entity);
                }
            }

            if (movedList.Any())
            {
                var names = string.Join(", ", movedList.Select(c => $"\"{c.Number}\""));
                var message = movedList.Count == 1
                    ? $"Đã đưa thẻ BHYT {names} thành công vào thùng rác"
                    : $"Đã đưa các thẻ BHYT: {names} thành công vào thùng rác";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy thẻ BHYT nào để đưa vào thùng rác.";
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
            var restoredEntity = new List<HealthInsurance>();
            foreach (var id in ids)
            {
                var entity = await _healthInsuranceRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    entity.IsActive = true;
                    entity.UpdatedBy = "Admin";
                    entity.UpdatedDate = DateTime.UtcNow;
                    await _healthInsuranceRepository.UpdateAsync(entity);
                    restoredEntity.Add(entity);
                }
            }

            if (restoredEntity.Any())
            {
                var names = string.Join(", ", restoredEntity.Select(c => $"\"{c.Number}\""));
                var message = restoredEntity.Count == 1
                    ? $"Đã khôi phục thẻ BHYT {names} thành công."
                    : $"Đã khôi phục các thẻ BHYT: {names} thành công.";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy thẻ BHYT nào để khôi phục.";
            }

            return RedirectToAction("Trash");
        }
    }
}
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Staff.Models.DTOs;
using Project.Areas.Staff.Models.Entities;
using Project.Helpers;
using Project.Repositories.Interfaces;
using Project.Services.Interfaces;

namespace Project.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Admin, Bacsi, Yta")]
    public class PatientsController : Controller
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;
        private readonly IImageService _imgService;
        private readonly IMapper _mapper;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly CodeGeneratorHelper _codeGenerator;

        public PatientsController
        (
            IPatientRepository patientRepository,
            IUserRepository userRepository,
            ITreatmentRecordRepository treatmentRecordRepository,
            IImageService imgService,
            IMapper mapper,
            ViewBagHelper viewBagHelper,
            CodeGeneratorHelper codeGenerator
        )
        {
            _patientRepository = patientRepository;
            _userRepository = userRepository;
            _treatmentRecordRepository = treatmentRecordRepository;
            _imgService = imgService;
            _mapper = mapper;
            _viewBagHelper = viewBagHelper;
            _codeGenerator = codeGenerator;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _patientRepository.GetAllAsync();
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(list);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var patient = await _patientRepository.GetByIdAdvancedAsync(id);
            if (patient == null)
            {
                return NotFound();
            }
            return View(patient);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var entity = await _patientRepository.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<PatientDto>(entity);

            ViewBag.PatientId = entity.Id;
            ViewBag.ExistingImage = entity.Images;

            await _viewBagHelper.BaseViewBag(ViewData);

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] PatientDto inputDto, Guid Id)
        {
            try
            {
                var entity = await _patientRepository.GetByIdAsync(Id);
                if (entity == null) return NotFound();

                _mapper.Map(inputDto, entity);
                entity.UpdatedBy = "Admin";
                entity.UpdatedDate = DateTime.UtcNow;

                if (inputDto.ImageFile != null && inputDto.ImageFile.Length > 0)
                {
                    entity.Images = await _imgService.SaveImageAsync(inputDto.ImageFile, "Patients");
                }

                await _patientRepository.UpdateAsync(entity);
                return Json(new { success = true, message = "Cập nhật thông tin bệnh nhân thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật bệnh nhân: " + ex.Message });
            }
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

            var treatmentRecords = new List<TreatmentRecord>();
            foreach (var id in ids)
            {
                var patient = await _patientRepository.GetByIdAsync(id);
                if (patient == null) continue;

                var patientTreatmentRecords = await _treatmentRecordRepository.GetByPatientIdAsync(id);
                if (patientTreatmentRecords.Any())
                {
                    treatmentRecords.AddRange(patientTreatmentRecords);
                }
            }

            if (treatmentRecords.Any())
            {
                var names = string.Join(", ", treatmentRecords.Select(c => $"\"{c.Patient.Name}\"").Distinct());
                var message = treatmentRecords.Select(c => c.PatientId).Distinct().Count() == 1
                    ? $"Không thể xóa bệnh nhân {names} vì vẫn còn lưu trữ hồ sơ khám bệnh."
                    : $"Không thể xóa các bệnh nhân: {names} vì vẫn còn lưu trữ hồ sơ khám bệnh.";
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Index");
            }

            var delList = new List<Patient>();
            foreach (var id in ids)
            {
                var entity = await _patientRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    await _patientRepository.DeleteAsync(id);
                    delList.Add(entity);
                }
            }

            if (delList.Any())
            {
                var names = string.Join(", ", delList.Select(c => $"\"{c.Name}\""));
                var message = delList.Count == 1
                    ? $"Đã xóa bệnh nhân {names} thành công"
                    : $"Đã xóa các bệnh nhân: {names} thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy bệnh nhân nào để xóa.";
            }

            return RedirectToAction("Index");
        }
    }
}

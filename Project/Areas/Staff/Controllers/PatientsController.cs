using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.Staff.Models.DTOs;
using Project.Areas.Staff.Models.Entities;
using Project.Helpers;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using Project.Services.Interfaces;

namespace Project.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Admin, Nhanvien")]
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
            var activeList = list.Where(x => x.IsActive == true).ToList();
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(activeList);
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
        public async Task<IActionResult> Create()
        {
            await _viewBagHelper.BaseViewBag(ViewData);
            var model = new PatientDto
            {
                Code = await _codeGenerator.GenerateUniqueCodeAsync(_patientRepository)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] PatientDto inputDto)
        {
            try
            {
                var entity = _mapper.Map<Patient>(inputDto);

                entity.CreatedBy = "Admin";
                entity.CreatedDate = DateTime.UtcNow;
                entity.IsActive = true;

                if (inputDto.ImageFile != null && inputDto.ImageFile.Length > 0)
                {
                    entity.Images = await _imgService.SaveImageAsync(inputDto.ImageFile, "Patients");
                }

                await _patientRepository.CreateAsync(entity);

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = entity.Code,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("11111111"),
                    Role = RoleType.Benhnhan,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "Admin",
                    IsActive = true,
                    PatientId = entity.Id,
                    IsFirstLogin = true
                };

                await _userRepository.CreateAsync(user);

                return Json(new { success = true, message = "Thêm bệnh nhân thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm bệnh nhân: " + ex.Message });
            }
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

        public async Task<IActionResult> Trash()
        {
            var list = await _patientRepository.GetAllAsync();
            var trashList = list.Where(x => x.IsActive == false).ToList();
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(trashList);
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
                    ? $"Không thể đưa bệnh nhân {names} vào thùng rác vì vẫn còn lưu trữ hồ sơ khám bệnh."
                    : $"Không thể đưa các bệnh nhân: {names} vào thùng rác vì vẫn còn lưu trữ hồ sơ khám bệnh.";
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Index");
            }

            var movedList = new List<Patient>();
            foreach (var id in ids)
            {
                var entity = await _patientRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    entity.IsActive = false;
                    entity.UpdatedBy = "Admin";
                    entity.UpdatedDate = DateTime.UtcNow;
                    await _patientRepository.UpdateAsync(entity);
                    movedList.Add(entity);
                }
            }

            if (movedList.Any())
            {
                var names = string.Join(", ", movedList.Select(c => $"\"{c.Name}\""));
                var message = movedList.Count == 1
                    ? $"Đã đưa bệnh nhân {names} thành công vào thùng rác"
                    : $"Đã đưa các bệnh nhân: {names} thành công vào thùng rác";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy bệnh nhân nào để đưa vào thùng rác.";
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
            var restoredEntity = new List<Patient>();
            foreach (var id in ids)
            {
                var entity = await _patientRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    entity.IsActive = true;
                    entity.UpdatedBy = "Admin";
                    entity.UpdatedDate = DateTime.UtcNow;
                    await _patientRepository.UpdateAsync(entity);
                    restoredEntity.Add(entity);
                }
            }

            if (restoredEntity.Any())
            {
                var names = string.Join(", ", restoredEntity.Select(c => $"\"{c.Name}\""));
                var message = restoredEntity.Count == 1
                    ? $"Đã khôi phục bệnh nhân {names} thành công."
                    : $"Đã khôi phục các bệnh nhân: {names} thành công.";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy bệnh nhân nào để khôi phục.";
            }

            return RedirectToAction("Trash");
        }
    }
}

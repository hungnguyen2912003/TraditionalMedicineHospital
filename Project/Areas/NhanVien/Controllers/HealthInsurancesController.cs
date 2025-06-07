using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.NhanVien.Models.DTOs;
using Project.Areas.NhanVien.Models.ViewModels;
using Project.Helpers;
using Project.Repositories.Interfaces;
using Project.Services.Features;

namespace Project.Areas.NhanVien.Controllers
{
    [Area("NhanVien")]
    [Authorize(Roles = "NhanVienHanhChinh")]
    [Route("bao-hiem-y-te")]
    public class HealthInsurancesController : Controller
    {
        private readonly IHealthInsuranceRepository _healthInsuranceRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly JwtManager _jwtManager;
        private readonly IMapper _mapper;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly CodeGeneratorHelper _codeGenerator;

        public HealthInsurancesController
        (
            IHealthInsuranceRepository healthInsuranceRepository,
            IPatientRepository patientRepository,
            IUserRepository userRepository,
            IEmployeeRepository employeeRepository,
            JwtManager jwtManager,
            IMapper mapper,
            ViewBagHelper viewBagHelper,
            CodeGeneratorHelper codeGenerator
        )
        {
            _healthInsuranceRepository = healthInsuranceRepository;
            _patientRepository = patientRepository;
            _userRepository = userRepository;
            _employeeRepository = employeeRepository;
            _jwtManager = jwtManager;
            _mapper = mapper;
            _viewBagHelper = viewBagHelper;
            _codeGenerator = codeGenerator;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _healthInsuranceRepository.GetAllAdvancedAsync();
            var viewModelList = _mapper.Map<List<HealthInsuranceViewModel>>(list);
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(viewModelList);
        }

        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var healthInsurance = await _healthInsuranceRepository.GetByIdAdvancedAsync(id);
            if (healthInsurance == null)
            {
                return NotFound();
            }
            var createdByEmployee = await _employeeRepository.GetByCodeAsync(healthInsurance.CreatedBy);
            var updatedByEmployee = await _employeeRepository.GetByCodeAsync(healthInsurance.UpdatedBy!);
            ViewBag.CreatedByEmployee = createdByEmployee?.Name ?? "Không có";
            ViewBag.UpdatedByEmployee = updatedByEmployee?.Name ?? "Không có";
            return View(healthInsurance);
        }

        [HttpGet("them-moi")]
        public async Task<IActionResult> Create()
        {
            await _viewBagHelper.GetPatientsWithoutInsurance(ViewData);
            var model = new HealthInsuranceDto
            {
                Code = await _codeGenerator.GenerateUniqueCodeAsync(_healthInsuranceRepository)
            };
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(model);
        }

        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] HealthInsuranceDto inputDto)
        {
            try
            {
                // Get user info from token
                var token = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Người dùng chưa đăng nhập" });
                }

                var (username, role) = _jwtManager.GetClaimsFromToken(token);
                if (string.IsNullOrEmpty(username))
                {
                    Response.Cookies.Delete("AuthToken");
                    return Json(new { success = false, message = "Token không hợp lệ." });
                }

                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null || user.Employee == null)
                {
                    return Json(new { success = false, message = "Người dùng không hợp lệ" });
                }

                // Validate that the patient exists
                var patient = await _patientRepository.GetByIdAsync(inputDto.PatientId);
                if (patient == null)
                {
                    return Json(new { success = false, message = "Bệnh nhân không tồn tại trong hệ thống." });
                }

                var entity = _mapper.Map<HealthInsurance>(inputDto);

                entity.CreatedBy = user.Employee.Code;
                entity.CreatedDate = DateTime.UtcNow;
                entity.PatientId = inputDto.PatientId; // Explicitly set PatientId

                await _healthInsuranceRepository.CreateAsync(entity);

                return Json(new { success = true, message = "Thêm thẻ BHYT thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm thẻ BHYT: " + ex.Message });
            }
        }

        [HttpGet("chinh-sua/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var entity = await _healthInsuranceRepository.GetByIdAdvancedAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<HealthInsuranceDto>(entity);

            ViewBag.HealthInsuranceId = entity.Id;

            await _viewBagHelper.BaseViewBag(ViewData);

            return View(dto);
        }

        [HttpPost("chinh-sua/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] HealthInsuranceDto inputDto, Guid Id)
        {
            try
            {
                // Get user info from token
                var token = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Người dùng chưa đăng nhập" });
                }

                var (username, role) = _jwtManager.GetClaimsFromToken(token);
                if (string.IsNullOrEmpty(username))
                {
                    Response.Cookies.Delete("AuthToken");
                    return Json(new { success = false, message = "Token không hợp lệ." });
                }

                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null || user.Employee == null)
                {
                    return Json(new { success = false, message = "Người dùng không hợp lệ" });
                }

                var entity = await _healthInsuranceRepository.GetByIdAdvancedAsync(Id);
                if (entity == null) return NotFound();

                // Log current state
                var originalPatientId = entity.PatientId;

                // Map the DTO to entity but exclude PatientId
                _mapper.Map(inputDto, entity);

                // Ensure PatientId is preserved
                entity.PatientId = originalPatientId;

                entity.UpdatedBy = user.Employee.Code;
                entity.UpdatedDate = DateTime.UtcNow;

                // Validate that the patient still exists
                var patient = await _patientRepository.GetByIdAsync(originalPatientId);
                if (patient == null)
                {
                    return Json(new { success = false, message = "Bệnh nhân không tồn tại trong hệ thống." });
                }

                await _healthInsuranceRepository.UpdateAsync(entity);
                return Json(new { success = true, message = "Cập nhật thông tin thẻ BHYT thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật thẻ BHYT: " + ex.Message });
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
                    : $"Đã xóa các thẻ BHYT đã chọn thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy thẻ BHYT nào để xóa.";
            }

            return RedirectToAction("Index");
        }
    }
}
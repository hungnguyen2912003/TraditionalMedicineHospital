using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.NhanVien.Models.DTOs;
using Project.Helpers;
using Project.Repositories.Implementations;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using Project.Services.Interfaces;

namespace Project.Areas.NhanVien.Controllers
{
    [Area("NhanVien")]
    [Authorize(Roles = "NhanVienHanhChinh")]
    [Route("benh-nhan")]
    public class PatientsController : Controller
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserRepository _userRepository;
        private readonly JwtManager _jwtManager;
        private readonly IImageService _imgService;
        private readonly IMapper _mapper;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly CodeGeneratorHelper _codeGenerator;

        public PatientsController
        (
            IPatientRepository patientRepository,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository,
            JwtManager jwtManager,
            IImageService imgService,
            IMapper mapper,
            ViewBagHelper viewBagHelper,
            CodeGeneratorHelper codeGenerator
        )
        {
            _patientRepository = patientRepository;
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _jwtManager = jwtManager;
            _imgService = imgService;
            _mapper = mapper;
            _viewBagHelper = viewBagHelper;
            _codeGenerator = codeGenerator;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _patientRepository.GetAllAsync();
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(list);
        }

        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var patient = await _patientRepository.GetByIdAdvancedAsync(id);
            if (patient == null)
            {
                return NotFound();
            }
            var createdByEmployee = await _employeeRepository.GetByCodeAsync(patient.CreatedBy);
            var updatedByEmployee = await _employeeRepository.GetByCodeAsync(patient.UpdatedBy!);
            ViewBag.CreatedByEmployee = createdByEmployee?.Name ?? "Không có";
            ViewBag.UpdatedByEmployee = updatedByEmployee?.Name ?? "Không có";
            return View(patient);
        }

        [HttpGet("chinh-sua/{id}")]
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

        [HttpPost("chinh-sua/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] PatientDto inputDto, Guid Id)
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

                var entity = await _patientRepository.GetByIdAsync(Id);
                if (entity == null) return NotFound();

                _mapper.Map(inputDto, entity);
                entity.UpdatedBy = user.Employee.Code;
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
    }
}

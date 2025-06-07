using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.NhanVien.Models.DTOs;
using Project.Helpers;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using Project.Services.Interfaces;

namespace Project.Areas.NhanVien.Controllers
{
    [Area("NhanVien")]
    [Authorize(Roles = "NhanVienHanhChinh")]
    [Route("tiep-nhan-benh-nhan")]
    public class ReceptionsController : Controller
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IHealthInsuranceRepository _healthInsuranceRepository;
        private readonly IUserRepository _userRepository;
        private readonly CodeGeneratorHelper _codeGenerator;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly JwtManager _jwtManager;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly EmailService _emailService;

        public ReceptionsController(
            IPatientRepository patientRepository,
            IHealthInsuranceRepository healthInsuranceRepository,
            IUserRepository userRepository,
            CodeGeneratorHelper codeGenerator,
            ViewBagHelper viewBagHelper,
            JwtManager jwtManager,
            IMapper mapper,
            IImageService imageService,
            EmailService emailService
        )
        {
            _patientRepository = patientRepository;
            _healthInsuranceRepository = healthInsuranceRepository;
            _userRepository = userRepository;
            _codeGenerator = codeGenerator;
            _viewBagHelper = viewBagHelper;
            _jwtManager = jwtManager;
            _mapper = mapper;
            _imageService = imageService;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Generate code each entities

            var model = new ReceptionDto
            {
                Code = await _codeGenerator.GenerateNumericCodeAsync(_patientRepository),
                HealthInsuranceCode = await _codeGenerator.GenerateUniqueCodeAsync(_healthInsuranceRepository)
            };

            await _viewBagHelper.BaseViewBag(ViewData);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ReceptionDto dto)
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

                var employee = user.Employee;

                var patient = _mapper.Map<Patient>(dto);
                patient.CreatedBy = employee.Code;
                patient.CreatedDate = DateTime.Now;

                if (dto!.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    var imagePath = await _imageService.SaveImageAsync(dto.ImageFile, "Patients");
                    patient.Images = imagePath;
                }

                await _patientRepository.CreateAsync(patient);

                var patientId = patient.Id;

                // Tạo tài khoản cho bệnh nhân
                var userPatient = new User
                {
                    Id = Guid.NewGuid(),
                    Username = patient.Code,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("11111111"),
                    Role = RoleType.BenhNhan,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = employee.Code,
                    PatientId = patient.Id,
                    IsFirstLogin = true
                };
                await _userRepository.CreateAsync(userPatient);

                // Gửi email nếu có
                if (!string.IsNullOrEmpty(patient.EmailAddress))
                {
                    var subject = "Tài khoản bệnh nhân mới tại Bệnh viện Y học cổ truyền Nha Trang";
                    var body = $@"
                            <h2>Xin chào {patient.Name},</h2>
                            <p>Bạn đã được cấp tài khoản bệnh nhân tại hệ thống Bệnh viện Y học cổ truyền Nha Trang.</p>
                            <p>Tài khoản của bạn là:</p>
                            <p><b>Mã bệnh nhân (Username):</b> {patient.Code}</p>
                            <p><b>Mật khẩu mặc định:</b> 11111111</p>
                            <p>Bạn có thể sử dụng mã bệnh nhân hoặc email để đăng nhập vào hệ thống.</p>
                            <p>Vui lòng đăng nhập tại <a href='https://localhost:5285/login'>trang đăng nhập</a> và đổi mật khẩu ngay lần đầu đăng nhập.</p>
                            <p>Trân trọng,<br>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>
                        ";
                    _ = Task.Run(() => _emailService.SendEmailAsync(patient.EmailAddress, subject, body));
                }

                // Create health insurance
                HealthInsurance? healthInsurance = null;
                if (dto.HasHealthInsurance)
                {
                    healthInsurance = _mapper.Map<HealthInsurance>(dto);
                    if (healthInsurance != null)
                    {
                        healthInsurance.PatientId = patientId;
                        healthInsurance.CreatedBy = employee.Code;
                        healthInsurance.CreatedDate = DateTime.Now;

                        await _healthInsuranceRepository.CreateAsync(healthInsurance);
                    }
                }

                return Json(new { success = true, message = "Tiếp nhận bệnh nhân thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

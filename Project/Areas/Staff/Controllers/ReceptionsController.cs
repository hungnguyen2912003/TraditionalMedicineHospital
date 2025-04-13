using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Staff.Models.DTOs;
using Project.Areas.Staff.Models.Entities;
using Project.Helpers;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using Project.Services.Implementations;
using Project.Services.Interfaces;

namespace Project.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Nhanvien")]
    public class ReceptionsController : Controller
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IHealthInsuranceRepository _healthInsuranceRepository;
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;
        private readonly ITreatmentRecordDetailRepository _treatmentRecordDetailRepository;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly ITreatmentRecordRegulationRepository _treatmentRecordRegulationRepository;
        private readonly ITreatmentMethodRepository _treatmentMethodRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IRegulationRepository _regulationRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserRepository _userRepository;
        private readonly CodeGeneratorHelper _codeGenerator;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly JwtManager _jwtManager;
        private readonly IMapper _mapper;
        private readonly IImageService _imgService;

        public ReceptionsController
        (
            IPatientRepository patientRepository,
            IHealthInsuranceRepository healthInsuranceRepository,
            ITreatmentRecordRepository treatmentRecordRepository,
            ITreatmentRecordDetailRepository treatmentRecordDetailRepository,
            IAssignmentRepository assignmentRepository,
            ITreatmentRecordRegulationRepository treatmentRecordRegulationRepository,
            ITreatmentMethodRepository treatmentMethodRepository,
            IRoomRepository roomRepository,
            IRegulationRepository regulationRepository,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository,
            CodeGeneratorHelper codeGenerator,
            ViewBagHelper viewBagHelper,
            JwtManager jwtManager,
            IMapper mapper,
            IImageService imgService
        )
        {
            _patientRepository = patientRepository;
            _healthInsuranceRepository = healthInsuranceRepository;
            _treatmentRecordRepository = treatmentRecordRepository;
            _treatmentRecordDetailRepository = treatmentRecordDetailRepository;
            _assignmentRepository = assignmentRepository;
            _treatmentRecordRegulationRepository = treatmentRecordRegulationRepository;
            _treatmentMethodRepository = treatmentMethodRepository;
            _roomRepository = roomRepository;
            _regulationRepository = regulationRepository;
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _codeGenerator = codeGenerator;
            _viewBagHelper = viewBagHelper;
            _jwtManager = jwtManager;
            _mapper = mapper;
            _imgService = imgService;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }

            await _viewBagHelper.BaseViewBag(ViewData, token);

            // Kiểm tra xem có thông tin bác sĩ không
            if (ViewData["EmployeeId"] == null || ViewData["DepartmentId"] == null)
            {
                Response.Cookies.Delete("AuthToken");
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }
            var model = new ReceptionDto
            {
                Patient = new ReceptionPatientDto
                {
                    Code = await _codeGenerator.GenerateUniqueCodeAsync(_patientRepository),
                    HealthInsuranceCode = await _codeGenerator.GenerateUniqueCodeAsync(_healthInsuranceRepository)
                },
                TreatmentRecord = new ReceptionTreatmentRecordDto
                {
                    Code = await _codeGenerator.GenerateUniqueCodeAsync(_treatmentRecordRepository)
                },
                TreatmentRecordDetail = new ReceptionTreatmentRecordDetailDto
                {
                    Code = await _codeGenerator.GenerateUniqueCodeAsync(_treatmentRecordDetailRepository)
                },
                Assignment = new ReceptionAssignmentDto
                {
                    Code = await _codeGenerator.GenerateUniqueCodeAsync(_assignmentRepository),
                    EmployeeId = (Guid)ViewData["EmployeeId"]
                }
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ReceptionDto dto)
        {
            try
            {
                // Lấy thông tin bác sĩ đang đăng nhập
                var token = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Chưa đăng nhập." });
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
                    return Json(new { success = false, message = "Không tìm thấy thông tin bác sĩ." });
                }

                var employee = user.Employee;

                // Phần 1: Lưu thông tin bệnh nhân
                var patient = _mapper.Map<Patient>(dto.Patient);
                patient.CreatedBy = employee.Name;
                patient.CreatedDate = DateTime.UtcNow;
                patient.IsActive = true;

                if (dto.Patient.ImageFile != null && dto.Patient.ImageFile.Length > 0)
                {
                    patient.Images = await _imgService.SaveImageAsync(dto.Patient.ImageFile, "Patients");
                }

                await _patientRepository.CreateAsync(patient);

                // Lưu thẻ BHYT nếu có
                HealthInsurance? healthInsurance = null;
                if (dto.Patient.HasHealthInsurance)
                {
                    healthInsurance = _mapper.Map<HealthInsurance>(dto.Patient);
                    healthInsurance.PatientId = patient.Id;
                    healthInsurance.CreatedBy = employee.Name;
                    healthInsurance.CreatedDate = DateTime.UtcNow;
                    healthInsurance.IsActive = true;
                    await _healthInsuranceRepository.CreateAsync(healthInsurance);
                }

                // Phần 2: Lưu TreatmentRecord
                var treatmentRecord = _mapper.Map<TreatmentRecord>(dto.TreatmentRecord);
                treatmentRecord.PatientId = patient.Id;
                treatmentRecord.CreatedBy = employee.Name;
                treatmentRecord.CreatedDate = DateTime.UtcNow;
                treatmentRecord.IsActive = true;
                await _treatmentRecordRepository.CreateAsync(treatmentRecord);

                // Phần 3: Lưu TreatmentRecordDetail
                var treatmentRecordDetail = _mapper.Map<TreatmentRecordDetail>(dto.TreatmentRecordDetail);
                treatmentRecordDetail.TreatmentRecordId = treatmentRecord.Id;
                treatmentRecordDetail.CreatedBy = employee.Name;
                treatmentRecordDetail.CreatedDate = DateTime.UtcNow;
                treatmentRecordDetail.IsActive = true;
                await _treatmentRecordDetailRepository.CreateAsync(treatmentRecordDetail);

                // Phần 4: Lưu Assignment
                var assignment = _mapper.Map<Assignment>(dto.Assignment);
                assignment.TreatmentRecordId = treatmentRecord.Id;
                assignment.EmployeeId = employee.Id;
                assignment.StartDate = dto.TreatmentRecord.StartDate;
                assignment.EndDate = dto.TreatmentRecord.EndDate;
                assignment.CreatedBy = employee.Name;
                assignment.CreatedDate = DateTime.UtcNow;
                assignment.IsActive = true;
                await _assignmentRepository.CreateAsync(assignment);

                // Phần 5: Lưu TreatmentRecord_Regulation
                foreach (var regulationDto in dto.Regulations)
                {
                    var regulation = _mapper.Map<TreatmentRecord_Regulation>(regulationDto);
                    regulation.TreatmentRecordId = treatmentRecord.Id;
                    regulation.Code = await _codeGenerator.GenerateUniqueCodeAsync(_treatmentRecordRegulationRepository);
                    regulation.CreatedBy = employee.Name;
                    regulation.CreatedDate = DateTime.UtcNow;
                    regulation.IsActive = true;
                    await _treatmentRecordRegulationRepository.CreateAsync(regulation);
                }

                return Json(new
                {
                    success = true,
                    message = "Tiếp nhận bệnh nhân và lập phiếu điều trị thành công!",
                    redirectUrl = Url.Action("Index", "Home", new { area = "Staff" })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Staff.Models.DTOs.ReceptionDTO;
using Project.Areas.Staff.Models.Entities;
using Project.Helpers;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using Project.Services.Interfaces;
using Project.Areas.Admin.Models.Entities;

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
        private readonly IUserRepository _userRepository;
        private readonly CodeGeneratorHelper _codeGenerator;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly JwtManager _jwtManager;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly IRoomRepository _roomRepository;
        public ReceptionsController(
            IPatientRepository patientRepository,
            IHealthInsuranceRepository healthInsuranceRepository,
            ITreatmentRecordRepository treatmentRecordRepository,
            ITreatmentRecordDetailRepository treatmentRecordDetailRepository,
            IAssignmentRepository assignmentRepository,
            ITreatmentRecordRegulationRepository treatmentRecordRegulationRepository,
            IUserRepository userRepository,
            CodeGeneratorHelper codeGenerator,
            ViewBagHelper viewBagHelper,
            JwtManager jwtManager,
            IMapper mapper,
            IImageService imageService,
            IRoomRepository roomRepository
        )
        {
            _patientRepository = patientRepository;
            _healthInsuranceRepository = healthInsuranceRepository;
            _treatmentRecordRepository = treatmentRecordRepository;
            _treatmentRecordDetailRepository = treatmentRecordDetailRepository;
            _userRepository = userRepository;
            _assignmentRepository = assignmentRepository;
            _treatmentRecordRegulationRepository = treatmentRecordRegulationRepository;
            _codeGenerator = codeGenerator;
            _viewBagHelper = viewBagHelper;
            _jwtManager = jwtManager;
            _mapper = mapper;
            _imageService = imageService;
            _roomRepository = roomRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Token
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }

            await _viewBagHelper.BaseViewBag(ViewData, token);

            if (ViewData["UserId"] == null || ViewData["DepId"] == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }

            // Generate code each entities

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
                    Code = await _codeGenerator.GenerateUniqueCodeAsync(_assignmentRepository)
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

                // Create patient
                var patient = _mapper.Map<Patient>(dto.Patient);
                patient.CreatedBy = employee.Name;
                patient.CreatedDate = DateTime.Now;
                patient.IsActive = true;

                if (dto.Patient.ImageFile != null && dto.Patient.ImageFile.Length > 0)
                {
                    var imagePath = await _imageService.SaveImageAsync(dto.Patient.ImageFile, "Patients");
                    patient.Images = imagePath;
                }

                await _patientRepository.CreateAsync(patient);

                // Create health insurance
                HealthInsurance? healthInsurance = null;
                if (dto.Patient.HasHealthInsurance)
                {
                    healthInsurance = _mapper.Map<HealthInsurance>(dto.Patient);
                    if (healthInsurance != null)
                    {
                        healthInsurance.PatientId = patient.Id;
                        healthInsurance.CreatedBy = employee.Name;
                        healthInsurance.CreatedDate = DateTime.Now;
                        healthInsurance.IsActive = true;

                        await _healthInsuranceRepository.CreateAsync(healthInsurance);
                    }
                }

                // Create treatment record
                var treatmentRecord = _mapper.Map<TreatmentRecord>(dto.TreatmentRecord);
                treatmentRecord.PatientId = patient.Id;
                treatmentRecord.CreatedBy = employee.Name;
                treatmentRecord.CreatedDate = DateTime.Now;
                treatmentRecord.IsActive = true;
                treatmentRecord.Status = TreatmentStatus.DangDieuTri;

                await _treatmentRecordRepository.CreateAsync(treatmentRecord);

                // Create treatment record detail
                var treatmentRecordDetail = _mapper.Map<TreatmentRecordDetail>(dto.TreatmentRecordDetail);
                treatmentRecordDetail.TreatmentRecordId = treatmentRecord.Id;
                treatmentRecordDetail.CreatedBy = employee.Name;
                treatmentRecordDetail.CreatedDate = DateTime.Now;
                treatmentRecordDetail.IsActive = true;

                // Validate RoomId is not empty
                if (treatmentRecordDetail.RoomId == Guid.Empty)
                {
                    return Json(new { success = false, message = "Vui lòng chọn phòng điều trị" });
                }

                // Validate RoomId exists
                var room = await _roomRepository.GetByIdAsync(treatmentRecordDetail.RoomId);
                if (room == null)
                {
                    return Json(new { success = false, message = $"Phòng điều trị với ID {treatmentRecordDetail.RoomId} không tồn tại" });
                }

                await _treatmentRecordDetailRepository.CreateAsync(treatmentRecordDetail);

                // Create treatment record regulations
                if (dto.Regulations != null && dto.Regulations.Any())
                {
                    foreach (var regulationDto in dto.Regulations)
                    {
                        var treatmentRecordRegulation = _mapper.Map<TreatmentRecord_Regulation>(regulationDto);
                        treatmentRecordRegulation.TreatmentRecordId = treatmentRecord.Id;
                        treatmentRecordRegulation.CreatedBy = employee.Name;
                        treatmentRecordRegulation.CreatedDate = DateTime.Now;
                        treatmentRecordRegulation.IsActive = true;

                        await _treatmentRecordRegulationRepository.CreateAsync(treatmentRecordRegulation);
                    }
                }

                // Create assignment
                var assignment = _mapper.Map<Assignment>(dto.Assignment);
                assignment.TreatmentRecordId = treatmentRecord.Id;
                assignment.EmployeeId = employee.Id;
                assignment.CreatedBy = employee.Name;
                assignment.CreatedDate = DateTime.Now;
                assignment.IsActive = true;

                await _assignmentRepository.CreateAsync(assignment);

                return Json(new { success = true, message = "Tiếp nhận bệnh nhân và lập phiếu khám thành công" });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += "\nInner Exception: " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += "\nInner Inner Exception: " + ex.InnerException.InnerException.Message;
                    }
                }
                return Json(new { success = false, message = errorMessage });
            }
        }
    }
}

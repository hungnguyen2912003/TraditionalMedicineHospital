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
    [Authorize(Roles = "Bacsi")]
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
        private readonly IEmployeeRepository _employeeRepository;
        private readonly EmailService _emailService;
        private readonly ITreatmentMethodRepository _treatmentMethodRepository;
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
            IRoomRepository roomRepository,
            IEmployeeRepository employeeRepository,
            EmailService emailService,
            ITreatmentMethodRepository treatmentMethodRepository
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
            _employeeRepository = employeeRepository;
            _emailService = emailService;
            _treatmentMethodRepository = treatmentMethodRepository;
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
                    Code = await _codeGenerator.GenerateNumericCodeAsync(_patientRepository),
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

                Guid patientId;
                if (dto.Patient != null && dto.Patient.Id != Guid.Empty)
                {
                    // Bệnh nhân cũ
                    var existingPatient = await _patientRepository.GetByIdAsync(dto.Patient.Id!.Value);
                    if (existingPatient == null)
                        return Json(new { success = false, message = "Bệnh nhân không tồn tại" });
                    patientId = existingPatient.Id;
                }
                else
                {
                    // Bệnh nhân mới
                    var patient = _mapper.Map<Patient>(dto.Patient);
                    patient.CreatedBy = employee.Code;
                    patient.CreatedDate = DateTime.Now;
                    patient.IsActive = true;

                    if (dto.Patient!.ImageFile != null && dto.Patient.ImageFile.Length > 0)
                    {
                        var imagePath = await _imageService.SaveImageAsync(dto.Patient.ImageFile, "Patients");
                        patient.Images = imagePath;
                    }

                    await _patientRepository.CreateAsync(patient);
                    patientId = patient.Id;

                    // Tạo tài khoản cho bệnh nhân
                    var userPatient = new User
                    {
                        Id = Guid.NewGuid(),
                        Username = patient.Code,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("11111111"),
                        Role = RoleType.Benhnhan,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = employee.Code,
                        IsActive = true,
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
                }

                // Create health insurance
                HealthInsurance? healthInsurance = null;
                if (dto.Patient.HasHealthInsurance)
                {
                    healthInsurance = _mapper.Map<HealthInsurance>(dto.Patient);
                    if (healthInsurance != null)
                    {
                        healthInsurance.PatientId = patientId;
                        healthInsurance.CreatedBy = employee.Code;
                        healthInsurance.CreatedDate = DateTime.Now;
                        healthInsurance.IsActive = true;

                        await _healthInsuranceRepository.CreateAsync(healthInsurance);
                    }
                }

                // Create treatment record
                var treatmentRecord = _mapper.Map<TreatmentRecord>(dto.TreatmentRecord);
                treatmentRecord.PatientId = patientId;
                treatmentRecord.CreatedBy = employee.Code;
                treatmentRecord.CreatedDate = DateTime.Now;
                treatmentRecord.IsActive = true;
                treatmentRecord.Status = TreatmentStatus.DangDieuTri;

                await _treatmentRecordRepository.CreateAsync(treatmentRecord);

                // Create all treatment record details
                if (dto.TreatmentRecordDetails != null && dto.TreatmentRecordDetails.Any())
                {
                    foreach (var detailDto in dto.TreatmentRecordDetails)
                    {
                        if (string.IsNullOrEmpty(detailDto.RoomId.ToString()) || detailDto.RoomId == Guid.Empty)
                        {
                            return Json(new { success = false, message = "Vui lòng chọn phòng điều trị" });
                        }
                        if (string.IsNullOrEmpty(detailDto.TreatmentMethodId.ToString()) || detailDto.TreatmentMethodId == Guid.Empty)
                        {
                            return Json(new { success = false, message = "Vui lòng chọn phương pháp điều trị" });
                        }

                        var treatmentRecordDetail = _mapper.Map<TreatmentRecordDetail>(detailDto);
                        treatmentRecordDetail.TreatmentRecordId = treatmentRecord.Id;
                        treatmentRecordDetail.RoomId = detailDto.RoomId;
                        treatmentRecordDetail.CreatedBy = employee.Code;
                        treatmentRecordDetail.CreatedDate = DateTime.Now;
                        treatmentRecordDetail.IsActive = true;

                        // Validate RoomId exists
                        var room = await _roomRepository.GetByIdAsync(treatmentRecordDetail.RoomId);
                        if (room == null)
                        {
                            return Json(new { success = false, message = $"Phòng điều trị với ID {treatmentRecordDetail.RoomId} không tồn tại" });
                        }

                        await _treatmentRecordDetailRepository.CreateAsync(treatmentRecordDetail);
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Vui lòng nhập ít nhất một chi tiết điều trị" });
                }

                // Create treatment record regulations
                if (dto.Regulations != null && dto.Regulations.Any())
                {
                    foreach (var regulationDto in dto.Regulations)
                    {
                        if (string.IsNullOrEmpty(regulationDto.RegulationId.ToString()) || regulationDto.RegulationId == Guid.Empty)
                        {
                            return Json(new { success = false, message = "Vui lòng chọn quy định" });
                        }

                        var treatmentRecordRegulation = _mapper.Map<TreatmentRecord_Regulation>(regulationDto);
                        treatmentRecordRegulation.TreatmentRecordId = treatmentRecord.Id;
                        treatmentRecordRegulation.CreatedBy = employee.Code;
                        treatmentRecordRegulation.CreatedDate = DateTime.Now;
                        treatmentRecordRegulation.IsActive = true;

                        await _treatmentRecordRegulationRepository.CreateAsync(treatmentRecordRegulation);
                    }
                }

                // Create assignment
                var assignment = _mapper.Map<Assignment>(dto.Assignment);
                assignment.TreatmentRecordId = treatmentRecord.Id;
                assignment.EmployeeId = employee.Id;
                assignment.CreatedBy = employee.Code;
                assignment.CreatedDate = DateTime.Now;
                assignment.IsActive = true;

                await _assignmentRepository.CreateAsync(assignment);

                return Json(new { success = true, message = "Tiếp nhận bệnh nhân và lập phiếu khám thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
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

            // Get current user
            var (username, role) = _jwtManager.GetClaimsFromToken(token);
            var user = await _userRepository.GetByUsernameAsync(username!);
            if (user == null || user.Employee == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }

            // Get treatment record
            var treatmentRecord = await _treatmentRecordRepository.GetByIdAsync(id);
            if (treatmentRecord == null)
            {
                return NotFound();
            }

            // Get patient
            var patient = await _patientRepository.GetByIdAsync(treatmentRecord.PatientId);
            if (patient == null)
            {
                return NotFound();
            }

            // Get health insurance
            var healthInsurance = await _healthInsuranceRepository.GetByPatientIdAsync(patient.Id);

            // Get treatment record details
            var details = await _treatmentRecordDetailRepository.GetByTreatmentRecordIdAsync(treatmentRecord.Id);
            var detailDtos = _mapper.Map<List<ReceptionTreatmentRecordDetailDto>>(details);

            // Get employees
            var employeeCodes = details.Select(x => x.CreatedBy).Distinct().ToList();
            var employees = await _employeeRepository.GetByCodesAsync(employeeCodes);

            // Map employees to detailDtos
            foreach (var dto in detailDtos)
            {
                var emp = employees.FirstOrDefault(e => e.Code == dto.CreatedBy);
                dto.EmployeeName = emp?.Name ?? "Không rõ";
                dto.EmployeeId = emp?.Id ?? Guid.Empty;
            }

            // Get assignments
            var assignments = await _assignmentRepository.GetByTreatmentRecordIdAsync(treatmentRecord.Id);

            // Get regulations
            var regulations = await _treatmentRecordRegulationRepository.GetByTreatmentRecordIdAsync(treatmentRecord.Id);

            // // Get all treatment methods for all departments
            // var treatmentMethods = await _treatmentMethodRepository.GetAllAsync();
            // ViewBag.TreatmentMethods = treatmentMethods;

            // Map to DTOs
            var model = new ReceptionEditDto
            {
                Patient = _mapper.Map<ReceptionPatientDto>(patient),
                TreatmentRecord = _mapper.Map<ReceptionTreatmentRecordDto>(treatmentRecord),
                TreatmentRecordDetails = detailDtos,
                Assignments = _mapper.Map<List<ReceptionAssignmentDto>>(assignments),
                Regulations = _mapper.Map<List<ReceptionTreatmentRecordRegulationDto>>(regulations),
                NewTreatmentRecordDetail = new ReceptionTreatmentRecordDetailDto
                {
                    Code = await _codeGenerator.GenerateUniqueCodeAsync(_treatmentRecordDetailRepository)
                },
                NewAssignment = new ReceptionAssignmentDto
                {
                    Code = await _codeGenerator.GenerateUniqueCodeAsync(_assignmentRepository)
                },
                CurrentEmployeeId = user.Employee.Id
            };

            // Set health insurance data
            if (healthInsurance != null)
            {
                model.Patient.HasHealthInsurance = true;
                model.Patient.HealthInsuranceCode = healthInsurance.Code;
                model.Patient.HealthInsuranceNumber = healthInsurance.Number;
                model.Patient.HealthInsuranceExpiryDate = healthInsurance.ExpiryDate;
                model.Patient.HealthInsurancePlaceOfRegistration = healthInsurance.PlaceOfRegistration;
            }

            // Set ViewBag values for the view
            ViewBag.TreatmentRecordId = treatmentRecord.Id;
            ViewBag.PatientId = patient.Id;
            ViewBag.HealthInsuranceId = healthInsurance?.Id;
            ViewBag.ExistingImage = patient.Images;
            ViewBag.CurrentEmployeeCode = user.Employee.Code;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] ReceptionEditDto dto)
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

                // Check if user has permission to edit this treatment record
                if (dto.TreatmentRecord == null)
                {
                    return Json(new { success = false, message = "Dữ liệu phiếu khám không hợp lệ" });
                }

                var treatmentRecord = await _treatmentRecordRepository.GetByIdAsync(dto.TreatmentRecord.Id);
                if (treatmentRecord == null)
                {
                    return Json(new { success = false, message = "Phiếu khám không tồn tại" });
                }

                // Check if treatment record is in a valid state for editing
                if (treatmentRecord.Status == TreatmentStatus.DaHuyBo)
                {
                    return Json(new { success = false, message = "Không thể cập nhật phiếu khám đã hủy bỏ" });
                }

                // Update patient
                var patient = await _patientRepository.GetByIdAsync(dto.TreatmentRecord.PatientId);
                if (patient == null)
                {
                    return Json(new { success = false, message = "Bệnh nhân không tồn tại" });
                }

                if (dto.Patient == null)
                {
                    return Json(new { success = false, message = "Dữ liệu bệnh nhân không hợp lệ" });
                }

                _mapper.Map(dto.Patient, patient);
                patient.UpdatedBy = employee.Name;
                patient.UpdatedDate = DateTime.Now;

                if (dto.Patient.ImageFile != null && dto.Patient.ImageFile.Length > 0)
                {
                    patient.Images = await _imageService.SaveImageAsync(dto.Patient.ImageFile, "Patients");
                }

                await _patientRepository.UpdateAsync(patient);

                // Update health insurance
                var healthInsurance = await _healthInsuranceRepository.GetByPatientIdAsync(patient.Id);
                if (dto.Patient.HasHealthInsurance)
                {
                    if (healthInsurance == null)
                    {
                        healthInsurance = _mapper.Map<HealthInsurance>(dto.Patient);
                        healthInsurance.PatientId = patient.Id;
                        healthInsurance.CreatedBy = employee.Code;
                        healthInsurance.CreatedDate = DateTime.Now;
                        healthInsurance.IsActive = true;

                        await _healthInsuranceRepository.CreateAsync(healthInsurance);
                    }
                    else
                    {
                        _mapper.Map(dto.Patient, healthInsurance);
                        healthInsurance.UpdatedBy = employee.Name;
                        healthInsurance.UpdatedDate = DateTime.Now;

                        await _healthInsuranceRepository.UpdateAsync(healthInsurance);
                    }
                }
                else if (healthInsurance != null)
                {
                    healthInsurance.IsActive = false;
                    healthInsurance.UpdatedBy = employee.Name;
                    healthInsurance.UpdatedDate = DateTime.Now;

                    await _healthInsuranceRepository.UpdateAsync(healthInsurance);
                }

                // Create new assignment if provided
                if (dto.NewAssignment != null &&
                    (dto.NewAssignment.StartDate != default ||
                     dto.NewAssignment.EndDate != default ||
                     !string.IsNullOrEmpty(dto.NewAssignment.Note)))
                {
                    if (dto.NewAssignment.StartDate == default)
                    {
                        return Json(new { success = false, message = "Vui lòng chọn ngày bắt đầu" });
                    }

                    if (dto.NewAssignment.EndDate == default)
                    {
                        return Json(new { success = false, message = "Vui lòng chọn ngày kết thúc" });
                    }

                    if (dto.NewAssignment.EndDate < dto.NewAssignment.StartDate)
                    {
                        return Json(new { success = false, message = "Ngày kết thúc phải sau ngày bắt đầu" });
                    }

                    var assignment = _mapper.Map<Assignment>(dto.NewAssignment);
                    assignment.TreatmentRecordId = dto.TreatmentRecord.Id;
                    assignment.EmployeeId = employee.Id;
                    assignment.CreatedBy = employee.Code;
                    assignment.CreatedDate = DateTime.Now;
                    assignment.IsActive = true;

                    await _assignmentRepository.CreateAsync(assignment);
                }

                // Update regulations
                if (dto.Regulations != null)
                {
                    var existingRegulations = await _treatmentRecordRegulationRepository.GetByTreatmentRecordIdAsync(dto.TreatmentRecord.Id);
                    var existingRegulationIds = existingRegulations.Select(r => r.RegulationId).ToList();
                    var newRegulationIds = dto.Regulations.Select(r => r.RegulationId).ToList();

                    // 1. XÓA những quy định đã bị loại khỏi form
                    var toDelete = existingRegulations.Where(r => !newRegulationIds.Contains(r.RegulationId)).ToList();
                    foreach (var reg in toDelete)
                    {
                        await _treatmentRecordRegulationRepository.DeleteAsync(reg.Id);
                    }

                    // 2. THÊM mới những quy định vừa thêm
                    foreach (var regulationDto in dto.Regulations)
                    {
                        if (!existingRegulationIds.Contains(regulationDto.RegulationId))
                        {
                            var treatmentRecordRegulation = _mapper.Map<TreatmentRecord_Regulation>(regulationDto);
                            treatmentRecordRegulation.TreatmentRecordId = dto.TreatmentRecord.Id;
                            treatmentRecordRegulation.CreatedBy = employee.Code;
                            treatmentRecordRegulation.CreatedDate = DateTime.Now;
                            treatmentRecordRegulation.IsActive = true;

                            if (string.IsNullOrWhiteSpace(treatmentRecordRegulation.Code))
                            {
                                treatmentRecordRegulation.Code = await _codeGenerator.GenerateUniqueCodeAsync(_treatmentRecordRegulationRepository);
                            }

                            await _treatmentRecordRegulationRepository.CreateAsync(treatmentRecordRegulation);
                        }
                        else
                        {
                            // Nếu muốn cập nhật Note hoặc ExecutionDate cho quy định cũ, có thể update ở đây
                            var existing = existingRegulations.FirstOrDefault(r => r.RegulationId == regulationDto.RegulationId);
                            if (existing != null)
                            {
                                existing.ExecutionDate = regulationDto.ExecutionDate;
                                existing.Note = regulationDto.Note;
                                existing.UpdatedBy = employee.Name;
                                existing.UpdatedDate = DateTime.Now;
                                await _treatmentRecordRegulationRepository.UpdateAsync(existing);
                            }
                        }
                    }
                }

                // 1. Lấy danh sách chi tiết cũ từ DB
                var oldDetails = await _treatmentRecordDetailRepository.GetByTreatmentRecordIdAsync(treatmentRecord.Id);
                var oldCodes = oldDetails.Select(d => d.Code).ToList();
                var newCodes = dto.TreatmentRecordDetails.Select(d => d.Code).ToList();

                // 2. XÓA các dòng đã bị loại khỏi form
                foreach (var old in oldDetails)
                {
                    if (!newCodes.Contains(old.Code))
                    {
                        await _treatmentRecordDetailRepository.DeleteAsync(old.Id);
                    }
                }

                // 3. THÊM mới các dòng chưa có trong DB
                foreach (var detailDto in dto.TreatmentRecordDetails)
                {
                    var old = oldDetails.FirstOrDefault(d => d.Code == detailDto.Code);
                    if (old == null)
                    {
                        // Thêm mới
                        var newDetail = _mapper.Map<TreatmentRecordDetail>(detailDto);
                        newDetail.TreatmentRecordId = treatmentRecord.Id;
                        newDetail.CreatedBy = employee.Code;
                        newDetail.CreatedDate = DateTime.Now;
                        newDetail.IsActive = true;
                        await _treatmentRecordDetailRepository.CreateAsync(newDetail);
                    }
                    else
                    {
                        // Sửa nếu có thay đổi
                        bool changed = old.RoomId != detailDto.RoomId
                                    || old.Note != detailDto.Note;
                        if (changed)
                        {
                            old.RoomId = detailDto.RoomId;
                            old.Note = detailDto.Note;
                            old.UpdatedBy = employee.Code;
                            old.UpdatedDate = DateTime.Now;
                            await _treatmentRecordDetailRepository.UpdateAsync(old);
                        }
                    }
                }

                return Json(new { success = true, message = "Cập nhật thông tin tiếp nhận thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

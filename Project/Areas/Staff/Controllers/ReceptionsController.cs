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
            var treatmentRecordDetails = await _treatmentRecordDetailRepository.GetByTreatmentRecordIdAsync(treatmentRecord.Id);

            // Get assignments
            var assignments = await _assignmentRepository.GetByTreatmentRecordIdAsync(treatmentRecord.Id);

            // Get regulations
            var regulations = await _treatmentRecordRegulationRepository.GetByTreatmentRecordIdAsync(treatmentRecord.Id);

            // Map to DTOs
            var model = new ReceptionEditDto
            {
                Patient = _mapper.Map<ReceptionPatientDto>(patient),
                TreatmentRecord = _mapper.Map<ReceptionTreatmentRecordDto>(treatmentRecord),
                TreatmentRecordDetails = _mapper.Map<List<ReceptionTreatmentRecordDetailDto>>(treatmentRecordDetails),
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

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] ReceptionEditDto dto)
        {
            try
            {
                // Log all incoming data
                Console.WriteLine("\n=== RECEPTION EDIT DATA START ===");

                // Patient Data
                Console.WriteLine("\n--- PATIENT INFO ---");
                Console.WriteLine($"Patient Code: {dto.Patient?.Code}");
                Console.WriteLine($"Patient Name: {dto.Patient?.Name}");
                Console.WriteLine($"Patient Gender: {dto.Patient?.Gender}");
                Console.WriteLine($"Patient DateOfBirth: {dto.Patient?.DateOfBirth}");
                Console.WriteLine($"Patient IdentityNumber: {dto.Patient?.IdentityNumber}");
                Console.WriteLine($"Patient PhoneNumber: {dto.Patient?.PhoneNumber}");
                Console.WriteLine($"Patient Address: {dto.Patient?.Address}");
                Console.WriteLine($"Patient EmailAddress: {dto.Patient?.EmailAddress}");

                // Health Insurance Data
                Console.WriteLine("\n--- HEALTH INSURANCE INFO ---");
                Console.WriteLine($"Has Health Insurance: {dto.Patient?.HasHealthInsurance}");
                if (dto.Patient?.HasHealthInsurance == true)
                {
                    Console.WriteLine($"Insurance Code: {dto.Patient?.HealthInsuranceCode}");
                    Console.WriteLine($"Insurance Number: {dto.Patient?.HealthInsuranceNumber}");
                    Console.WriteLine($"Insurance Expiry Date: {dto.Patient?.HealthInsuranceExpiryDate}");
                    Console.WriteLine($"Insurance Place: {dto.Patient?.HealthInsurancePlaceOfRegistration}");
                    Console.WriteLine($"Insurance Is Right Route: {dto.Patient?.HealthInsuranceIsRightRoute}");
                }

                // Treatment Record Data
                Console.WriteLine("\n--- TREATMENT RECORD INFO ---");
                Console.WriteLine($"Treatment Record ID: {dto.TreatmentRecord?.Id}");
                Console.WriteLine($"Treatment Record Code: {dto.TreatmentRecord?.Code}");
                Console.WriteLine($"Treatment Start Date: {dto.TreatmentRecord?.StartDate}");
                Console.WriteLine($"Treatment End Date: {dto.TreatmentRecord?.EndDate}");
                Console.WriteLine($"Treatment Diagnosis: {dto.TreatmentRecord?.Diagnosis}");
                Console.WriteLine($"Treatment Note: {dto.TreatmentRecord?.Note}");

                // New Treatment Record Detail
                if (dto.NewTreatmentRecordDetail != null)
                {
                    Console.WriteLine("\n--- NEW TREATMENT DETAIL ---");
                    Console.WriteLine($"Detail Code: {dto.NewTreatmentRecordDetail.Code}");
                    Console.WriteLine($"Treatment Method ID: {dto.NewTreatmentRecordDetail.TreatmentMethodId}");
                    Console.WriteLine($"Room ID: {dto.NewTreatmentRecordDetail.RoomId}");
                    Console.WriteLine($"Note: {dto.NewTreatmentRecordDetail.Note}");
                }

                // New Assignment
                if (dto.NewAssignment != null)
                {
                    Console.WriteLine("\n--- NEW ASSIGNMENT ---");
                    Console.WriteLine($"Assignment Code: {dto.NewAssignment.Code}");
                    Console.WriteLine($"Start Date: {dto.NewAssignment.StartDate}");
                    Console.WriteLine($"End Date: {dto.NewAssignment.EndDate}");
                    Console.WriteLine($"Note: {dto.NewAssignment.Note}");
                }

                // Regulations
                if (dto.Regulations != null && dto.Regulations.Any())
                {
                    Console.WriteLine("\n--- REGULATIONS ---");
                    foreach (var reg in dto.Regulations)
                    {
                        Console.WriteLine($"Regulation ID: {reg.RegulationId}, Date: {reg.ExecutionDate}");
                    }
                }

                Console.WriteLine("\n=== RECEPTION EDIT DATA END ===\n");

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
                var treatmentRecord = await _treatmentRecordRepository.GetByIdAsync(dto.TreatmentRecord.Id);
                if (treatmentRecord == null)
                {
                    return Json(new { success = false, message = "Phiếu khám không tồn tại" });
                }
                ViewBag.TreatmentRecordId = treatmentRecord.Id;

                // Check if treatment record is in a valid state for editing
                if (treatmentRecord.Status == TreatmentStatus.DaKetThuc)
                {
                    return Json(new { success = false, message = "Không thể cập nhật phiếu khám đã kết thúc" });
                }

                // Update patient
                var patient = await _patientRepository.GetByIdAsync(dto.TreatmentRecord.PatientId);
                if (patient == null)
                {
                    return Json(new { success = false, message = "Bệnh nhân không tồn tại" });
                }


                _mapper.Map(dto.Patient, patient);
                patient.UpdatedBy = employee.Name;
                patient.UpdatedDate = DateTime.Now;

                if (dto.Patient.ImageFile != null && dto.Patient.ImageFile.Length > 0)
                {
                    patient.Images = await _imageService.SaveImageAsync(dto.Patient.ImageFile, "Patients");
                }

                ViewBag.PatientId = patient.Id;
                ViewBag.ExistingImage = patient.Images;

                await _patientRepository.UpdateAsync(patient);

                // Update health insurance
                var healthInsurance = await _healthInsuranceRepository.GetByPatientIdAsync(patient.Id);
                if (dto.Patient.HasHealthInsurance)
                {
                    if (healthInsurance == null)
                    {
                        healthInsurance = _mapper.Map<HealthInsurance>(dto.Patient);
                        healthInsurance.PatientId = patient.Id;
                        healthInsurance.CreatedBy = employee.Name;
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



                // Create new treatment record detail if provided
                if (dto.NewTreatmentRecordDetail != null &&
                    (dto.NewTreatmentRecordDetail.TreatmentMethodId != Guid.Empty ||
                     dto.NewTreatmentRecordDetail.RoomId != Guid.Empty ||
                     !string.IsNullOrEmpty(dto.NewTreatmentRecordDetail.Note)))
                {
                    if (dto.NewTreatmentRecordDetail.TreatmentMethodId == Guid.Empty)
                    {
                        return Json(new { success = false, message = "Vui lòng chọn phương pháp điều trị" });
                    }

                    if (dto.NewTreatmentRecordDetail.RoomId == Guid.Empty)
                    {
                        return Json(new { success = false, message = "Vui lòng chọn phòng điều trị" });
                    }

                    var treatmentRecordDetail = _mapper.Map<TreatmentRecordDetail>(dto.NewTreatmentRecordDetail);
                    treatmentRecordDetail.TreatmentRecordId = dto.TreatmentRecord.Id;
                    treatmentRecordDetail.CreatedBy = employee.Name;
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
                    assignment.CreatedBy = employee.Name;
                    assignment.CreatedDate = DateTime.Now;
                    assignment.IsActive = true;

                    await _assignmentRepository.CreateAsync(assignment);
                }

                // Update regulations
                if (dto.Regulations != null && dto.Regulations.Any())
                {
                    var existingRegulations = await _treatmentRecordRegulationRepository.GetByTreatmentRecordIdAsync(dto.TreatmentRecord.Id);
                    var existingRegulationIds = existingRegulations.Select(r => r.RegulationId).ToList();

                    foreach (var regulationDto in dto.Regulations)
                    {
                        if (!existingRegulationIds.Contains(regulationDto.RegulationId))
                        {
                            var treatmentRecordRegulation = _mapper.Map<TreatmentRecord_Regulation>(regulationDto);
                            treatmentRecordRegulation.TreatmentRecordId = dto.TreatmentRecord.Id;
                            treatmentRecordRegulation.CreatedBy = employee.Name;
                            treatmentRecordRegulation.CreatedDate = DateTime.Now;
                            treatmentRecordRegulation.IsActive = true;

                            await _treatmentRecordRegulationRepository.CreateAsync(treatmentRecordRegulation);
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

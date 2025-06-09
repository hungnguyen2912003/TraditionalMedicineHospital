using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.BacSi.Models.DTOs;
using Project.Areas.BacSi.Models.ViewModels;
using Project.Helpers;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using System.Globalization;

namespace Project.Areas.BacSi.Controllers
{
    [Area("BacSi")]
    [Authorize(Roles = "BacSi")]
    [Route("phieu-dieu-tri")]
    public class TreatmentRecordsController : Controller
    {
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IHealthInsuranceRepository _healthInsuranceRepository;
        private readonly ITreatmentRecordDetailRepository _treatmentRecordDetailRepository;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly JwtManager _jwtManager;
        private readonly EmailService _emailService;
        private readonly IRoomRepository _roomRepository;
        private readonly ITreatmentRecordRegulationRepository _treatmentRecordRegulationRepository;
        private readonly IRegulationRepository _regulationRepository;
        private readonly IMapper _mapper;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly CodeGeneratorHelper _codeGenerator;
        public TreatmentRecordsController
        (
            ITreatmentRecordRepository treatmentRecordRepository,
            IUserRepository userRepository,
            IEmployeeRepository employeeRepository,
            IPatientRepository patientRepository,
            IHealthInsuranceRepository healthInsuranceRepository,
            ITreatmentRecordDetailRepository treatmentRecordDetailRepository,
            IAssignmentRepository assignmentRepository,
            JwtManager jwtManager,
            EmailService emailService,
            IRoomRepository roomRepository,
            ITreatmentRecordRegulationRepository treatmentRecordRegulationRepository,
            IRegulationRepository regulationRepository,
            IMapper mapper,
            ViewBagHelper viewBagHelper,
            CodeGeneratorHelper codeGenerator
        )
        {
            _treatmentRecordRepository = treatmentRecordRepository;
            _userRepository = userRepository;
            _employeeRepository = employeeRepository;
            _patientRepository = patientRepository;
            _healthInsuranceRepository = healthInsuranceRepository;
            _treatmentRecordDetailRepository = treatmentRecordDetailRepository;
            _assignmentRepository = assignmentRepository;
            _jwtManager = jwtManager;
            _emailService = emailService;
            _roomRepository = roomRepository;
            _treatmentRecordRegulationRepository = treatmentRecordRegulationRepository;
            _regulationRepository = regulationRepository;
            _mapper = mapper;
            _viewBagHelper = viewBagHelper;
            _codeGenerator = codeGenerator;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _treatmentRecordRepository.GetAllAdvancedAsync();
            var viewModelList = _mapper.Map<List<TreatmentRecordViewModel>>(list);
            viewModelList = viewModelList.Where(x => x.Status == TreatmentStatus.DangDieuTri).ToList();
            await _viewBagHelper.BaseViewBag(ViewData);
            var token = Request.Cookies["AuthToken"];
            if (!string.IsNullOrEmpty(token))
            {
                var (username, role) = _viewBagHelper._jwtManager.GetClaimsFromToken(token);
                if (!string.IsNullOrEmpty(username))
                {
                    var user = await _userRepository.GetByUsernameAsync(username);
                    if (user != null && user.Employee != null)
                    {
                        ViewBag.CurrentEmployeeCode = user.Employee.Code;
                        ViewBag.CurrentEmployeeName = user.Employee.Name;
                        ViewBag.CurrentRole = user.Role.ToString();
                    }
                }
            }

            foreach (var vm in viewModelList)
            {
                var entity = list.FirstOrDefault(x => x.Id == vm.Id);
                if (entity != null && entity.Assignments != null)
                {
                    vm.Assignments = entity.Assignments.Select(a => new Assignment
                    {
                        Id = a.Id,
                        CreatedBy = a.CreatedBy
                    }).ToList();
                }
                else
                {
                    vm.Assignments = new List<Assignment>();
                }
            }

            return View(viewModelList);
        }

        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var treatmentRecord = await _treatmentRecordRepository.GetByIdAdvancedAsync(id);
            if (treatmentRecord == null)
            {
                return NotFound();
            }
            return View(treatmentRecord);
        }

        [HttpGet("lap-phieu")]
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

            // Custom ViewBag.Patients logic
            var allPatients = await _patientRepository.GetAllAdvancedAsync();
            var patients = allPatients
                .Where(p =>
                    // New patient: no treatment records
                    p.TreatmentRecords == null || !p.TreatmentRecords.Any()
                    // Old patient: has no treatment record in status DangDieuTri
                    || (p.TreatmentRecords != null && !p.TreatmentRecords.Any(tr => tr.Status == TreatmentStatus.DangDieuTri))
                )
                .Select(p => new { p.Id, p.Name })
                .ToList();
            ViewBag.Patient_TreatmentRecord = patients;

            // Generate code each entities
            var model = new TreatmentRecordCreateDto
            {
                TreatmentRecord = new TreatmentRecordDto
                {
                    Code = await _codeGenerator.GenerateUniqueCodeAsync(_treatmentRecordRepository)
                },
                TreatmentRecordDetails = new List<TreatmentRecordDetailDto>
                {
                    new TreatmentRecordDetailDto
                    {
                        Code = await _codeGenerator.GenerateUniqueCodeAsync(_treatmentRecordDetailRepository)
                    }
                },
                Regulations = new List<Project.Areas.BacSi.Models.DTOs.RegulationDto>
                {
                    new Project.Areas.BacSi.Models.DTOs.RegulationDto
                    {
                        Code = await _codeGenerator.GenerateUniqueCodeAsync(_treatmentRecordRegulationRepository)
                    }
                },
                Assignment = new AssignmentDto
                {
                    Code = await _codeGenerator.GenerateUniqueCodeAsync(_assignmentRepository)
                }
            };

            return View(model);
        }

        [HttpPost("lap-phieu")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] TreatmentRecordCreateDto dto)
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


                // Create treatment record
                var treatmentRecord = _mapper.Map<TreatmentRecord>(dto.TreatmentRecord);
                treatmentRecord.PatientId = dto.TreatmentRecord.PatientId;
                treatmentRecord.CreatedBy = employee.Code;
                treatmentRecord.CreatedDate = DateTime.Now;
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

                        await _treatmentRecordRegulationRepository.CreateAsync(treatmentRecordRegulation);
                    }
                }

                // Create assignment
                var assignment = _mapper.Map<Assignment>(dto.Assignment);
                assignment.TreatmentRecordId = treatmentRecord.Id;
                assignment.EmployeeId = employee.Id;
                assignment.CreatedBy = employee.Code;
                assignment.CreatedDate = DateTime.Now;

                await _assignmentRepository.CreateAsync(assignment);

                return Json(new { success = true, message = "Lập phiếu điều trị thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("cap-nhat-phieu/{id}")]
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
            ViewBag.PatientName = patient.Name;
            var allRegulations = await _regulationRepository.GetAllAsync();
            ViewBag.Regulations = allRegulations.Select(r => new
            {
                id = r.Id,
                name = r.Name,
                effectiveStartDate = r.EffectiveDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                effectiveEndDate = r.ExpirationDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
            }).ToList();

            // Get treatment record details
            var details = await _treatmentRecordDetailRepository.GetByTreatmentRecordIdAsync(treatmentRecord.Id);
            var detailDtos = _mapper.Map<List<TreatmentRecordDetailDto>>(details);

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

            // Map to DTOs
            var model = new TreatmentRecordEditDto
            {
                TreatmentRecord = _mapper.Map<TreatmentRecordDto>(treatmentRecord),
                TreatmentRecordDetails = detailDtos,
                Assignments = _mapper.Map<List<AssignmentDto>>(assignments),
                Regulations = _mapper.Map<List<Project.Areas.BacSi.Models.DTOs.RegulationDto>>(regulations),
                NewTreatmentRecordDetail = new TreatmentRecordDetailDto
                {
                    Code = await _codeGenerator.GenerateUniqueCodeAsync(_treatmentRecordDetailRepository)
                },
                NewAssignment = new AssignmentDto
                {
                    Code = await _codeGenerator.GenerateUniqueCodeAsync(_assignmentRepository)
                },
                CurrentEmployeeId = user.Employee.Id
            };

            // Set ViewBag values for the view
            ViewBag.TreatmentRecordId = treatmentRecord.Id;
            ViewBag.PatientId = patient.Id;
            ViewBag.ExistingImage = patient.Images;
            ViewBag.CurrentEmployeeCode = user.Employee.Code;

            return View(model);
        }

        [HttpPost("cap-nhat-phieu/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] TreatmentRecordEditDto dto)
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

                _mapper.Map(dto.TreatmentRecord, treatmentRecord);
                treatmentRecord.UpdatedBy = employee.Code;
                treatmentRecord.UpdatedDate = DateTime.Now;

                await _treatmentRecordRepository.UpdateAsync(treatmentRecord);

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

                return Json(new { success = true, message = "Cập nhật thông tin phiếu điều trị thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("dinh-chi")]
        public async Task<IActionResult> SuspendedList()
        {
            var allRecords = await _treatmentRecordRepository.GetAllAdvancedAsync();
            var suspendedRecordsRaw = allRecords
                .Where(r => r.Status == TreatmentStatus.DaHoanThanh || r.Status == TreatmentStatus.DaHuyBo)
                .ToList();

            var suspendedByCodes = suspendedRecordsRaw
                .Where(r => !string.IsNullOrEmpty(r.SuspendedBy))
                .Select(r => r.SuspendedBy)
                .Distinct()
                .ToList();

            var employees = await _employeeRepository.GetByCodesAsync(suspendedByCodes!);
            var codeNameDict = employees.ToDictionary(e => e.Code, e => e.Name);

            var suspendedRecords = suspendedRecordsRaw
                .Select(r => new TreatmentSuspendedViewModel
                {
                    PatientName = r.Patient.Name,
                    TreatmentCode = r.Code,
                    SuspendedDate = r.SuspendedDate,
                    SuspendedBy = !string.IsNullOrEmpty(r.SuspendedBy) && codeNameDict.ContainsKey(r.SuspendedBy)
                        ? codeNameDict[r.SuspendedBy]
                        : r.SuspendedBy,
                    Reason = r.SuspendedReason
                })
                .ToList();
            return View(suspendedRecords);
        }
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Helpers;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using Project.Areas.BenhNhan.Models.ViewModels;
using Project.Models.Enums;

namespace Project.Areas.BenhNhan.Controllers
{
    [Area("BenhNhan")]
    [Authorize(Roles = "Benhnhan")]
    public class HomeController : Controller
    {
        private readonly IHealthInsuranceRepository _healthInsuranceRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;
        private readonly ITreatmentTrackingRepository _treatmentTrackingRepository;
        private readonly ITreatmentRecordDetailRepository _treatmentRecordDetailRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly JwtManager _jwtManager;
        private readonly IEmployeeRepository _employeeRepository;

        public HomeController
        (
            IHealthInsuranceRepository healthInsuranceRepository,
            IPatientRepository patientRepository,
            IMapper mapper,
            ViewBagHelper viewBagHelper,
            ITreatmentRecordRepository treatmentRecordRepository,
            ITreatmentTrackingRepository treatmentTrackingRepository,
            ITreatmentRecordDetailRepository treatmentRecordDetailRepository,
            IUserRepository userRepository,
            JwtManager jwtManager,
            IEmployeeRepository employeeRepository
        )
        {
            _healthInsuranceRepository = healthInsuranceRepository;
            _patientRepository = patientRepository;
            _mapper = mapper;
            _viewBagHelper = viewBagHelper;
            _treatmentRecordRepository = treatmentRecordRepository;
            _treatmentTrackingRepository = treatmentTrackingRepository;
            _treatmentRecordDetailRepository = treatmentRecordDetailRepository;
            _userRepository = userRepository;
            _jwtManager = jwtManager;
            _employeeRepository = employeeRepository;
        }

        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account", new { area = "Admin" });

            var (username, role) = _jwtManager.GetClaimsFromToken(token);
            if (string.IsNullOrEmpty(username) || role != "Benhnhan")
                return RedirectToAction("Login", "Account", new { area = "Admin" });

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || user.Patient == null)
                return RedirectToAction("Login", "Account", new { area = "Admin" });

            var patient = await _patientRepository.GetByIdAsync(user.Patient.Id);
            if (patient == null)
                return RedirectToAction("Login", "Account", new { area = "Admin" });

            var healthInsurance = await _healthInsuranceRepository.GetByPatientIdAsync(patient.Id);
            ViewBag.PatientInfo = patient;
            ViewBag.HealthInsurance = healthInsurance;

            // Get all treatment record details for patient's treatment records
            var treatmentRecords = await _treatmentRecordRepository.GetByPatientIdAsync(patient.Id);
            var latestRecord = treatmentRecords
                .OrderByDescending(r => r.StartDate)
                .FirstOrDefault();

            var viewModels = new List<PatientViewModel>();
            if (latestRecord != null)
            {
                var details = await _treatmentRecordDetailRepository.GetByTreatmentRecordIdAsync(latestRecord.Id);
                foreach (var detail in details)
                {
                    var doctorName = "Chưa phân công";
                    if (detail.TreatmentRecord?.Assignments != null)
                    {
                        var assignment = detail.TreatmentRecord.Assignments.FirstOrDefault();
                        if (assignment?.Employee != null)
                            doctorName = assignment.Employee.Name;
                    }

                    var departmentName = detail.Room?.Department?.Name ?? "Chưa xác định";
                    var treatmentMethodName = detail.Room?.TreatmentMethod?.Name ?? "Chưa phân công";
                    var roomName = detail.Room?.Name ?? "Chưa xác định";

                    var viewModel = new PatientViewModel
                    {
                        Id = detail.Id,
                        Code = detail.Code,
                        TreatmentRecordCode = latestRecord.Code,
                        PatientName = patient.Name,
                        DoctorName = doctorName,
                        DepartmentName = departmentName,
                        TreatmentMethodName = treatmentMethodName,
                        RoomName = roomName,
                        StartDate = latestRecord.StartDate,
                        EndDate = latestRecord.EndDate,
                        Status = latestRecord.Status,
                        Note = detail.Note
                    };
                    viewModels.Add(viewModel);
                }
                ViewBag.TreatmentRecordCode = latestRecord.Code;
                ViewBag.StartDate = latestRecord.StartDate;
                ViewBag.EndDate = latestRecord.EndDate;
                if (latestRecord.Status == TreatmentStatus.DangDieuTri)
                    ViewBag.Status = "Đang điều trị";
                else if (latestRecord.Status == TreatmentStatus.DaHoanThanh)
                    ViewBag.Status = "Đã hoàn tất";
                else if (latestRecord.Status == TreatmentStatus.DaHuyBo)
                    ViewBag.Status = "Đã hủy";
            }

            List<string> doctorNames = new List<string>();
            if (latestRecord?.Assignments != null)
            {
                doctorNames = latestRecord!.Assignments
                    .Where(a => a.Employee != null)
                    .Select(a => a.Employee.Name)
                    .Distinct()
                    .ToList();
            }

            ViewBag.DoctorNames = doctorNames;

            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> GetTreatmentTracking(Guid detailId)
        {
            var detail = await _treatmentRecordDetailRepository.GetByIdAdvancedAsync(detailId);
            if (detail == null) return NotFound();

            var logs = (await _treatmentTrackingRepository.GetByDetailIdAsync(detailId))
                .OrderBy(x => x.TrackingDate)
                .ToList();

            // Lấy tất cả EmployeeId duy nhất từ logs
            var employeeIds = logs
                .Where(x => x.EmployeeId != null)
                .Select(x => x.EmployeeId!.Value)
                .Distinct()
                .ToList();

            // Lấy thông tin nhân viên
            var employees = await _employeeRepository.GetByIdsAsync(employeeIds);
            var employeeDict = employees.ToDictionary(e => e.Id, e => e.Name);

            var result = new
            {
                methodName = detail.Room?.TreatmentMethod?.Name ?? "",
                roomName = detail.Room?.Name ?? "",
                logs = logs.Select(x => new
                {
                    date = x.TrackingDate.ToString("dd/MM/yyyy"),
                    status = x.Status,
                    staff = x.EmployeeId != null && employeeDict.ContainsKey(x.EmployeeId.Value)
                        ? employeeDict[x.EmployeeId.Value]
                        : "Không xác định",
                    note = x.Note
                })
            };
            return Json(result);
        }
    }
}

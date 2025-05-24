using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Staff.Models.ViewModels;
using Project.Helpers;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using Project.Services.Features;

namespace Project.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Route("canh-bao-benh-nhan")]
    public class WarningPatientsController : Controller
    {
        private readonly ITreatmentTrackingRepository _treatmentTrackingRepository;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly IMapper _mapper;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserRepository _userRepository;
        private readonly EmailService _emailService;

        public WarningPatientsController
        (
            ITreatmentTrackingRepository treatmentTrackingRepository,
            ViewBagHelper viewBagHelper,
            IMapper mapper,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository,
            EmailService emailService
        )
        {
            _treatmentTrackingRepository = treatmentTrackingRepository;
            _viewBagHelper = viewBagHelper;
            _mapper = mapper;
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Lấy tất cả tracking
            var allTrackings = await _treatmentTrackingRepository.GetAllAdvancedAsync();

            // Lấy mã nhân viên hiện tại từ cookie AuthToken
            string? currentEmployeeCode = null;
            var token = Request.Cookies["AuthToken"];
            if (!string.IsNullOrEmpty(token))
            {
                var (username, role) = _viewBagHelper._jwtManager.GetClaimsFromToken(token);
                if (!string.IsNullOrEmpty(username))
                {
                    var user = await _userRepository.GetByUsernameAsync(username);
                    if (user != null && user.Employee != null)
                    {
                        currentEmployeeCode = user.Employee.Code;
                        ViewBag.CurrentEmployeeCode = currentEmployeeCode;
                        ViewBag.CurrentRole = user.Role.ToString();
                    }
                }
            }

            // Nhóm các tracking theo bệnh nhân và từng đợt điều trị
            var patientGroups = allTrackings
                .Where(t => t.TreatmentRecordDetail?.TreatmentRecord?.Patient != null)
                .GroupBy(t => new
                {
                    PatientId = t.TreatmentRecordDetail!.TreatmentRecord!.Patient!.Id,
                    TreatmentRecordDetailId = t.TreatmentRecordDetailId
                })
                .Select(g => new
                {
                    PatientId = g.Key.PatientId,
                    TreatmentRecordDetailId = g.Key.TreatmentRecordDetailId,
                    PatientName = g.First().TreatmentRecordDetail!.TreatmentRecord!.Patient!.Name,
                    PatientEmail = g.First().TreatmentRecordDetail!.TreatmentRecord!.Patient!.EmailAddress,
                    Trackings = g.OrderBy(t => t.TrackingDate).ToList()
                })
                .ToList();

            // 1. Lấy tất cả EmployeeId từ các tracking cảnh báo
            var employeeIds = patientGroups
                .SelectMany(p => p.Trackings)
                .Where(t => t.EmployeeId.HasValue)
                .Select(t => t.EmployeeId!.Value)
                .Distinct()
                .ToList();
            var employeeDict = new Dictionary<Guid, string>();
            if (employeeIds.Any())
            {
                var employees = await _employeeRepository.GetByIdsAsync(employeeIds);
                employeeDict = employees.ToDictionary(e => e.Id, e => e.Name);
            }

            // Lọc ra các bệnh nhân có thể có nhiều lần cảnh báo (mỗi lần là một dòng)
            var warningPatients = new List<WarningPatientViewModel>();
            foreach (var p in patientGroups)
            {
                var ordered = p.Trackings;
                for (int i = 1; i < ordered.Count; i++)
                {
                    var prev = ordered[i - 1];
                    var curr = ordered[i];
                    var daysDiff = (curr.TrackingDate.Date - prev.TrackingDate.Date).TotalDays;
                    if (daysDiff == 1 &&
                        prev.Status == TrackingStatus.KhongDieuTri &&
                        curr.Status == TrackingStatus.KhongDieuTri)
                    {
                        warningPatients.Add(new WarningPatientViewModel
                        {
                            PatientId = p.PatientId,
                            PatientName = p.PatientName,
                            PatientEmail = p.PatientEmail,
                            FirstAbsenceDate = prev.TrackingDate,
                            SecondAbsenceDate = curr.TrackingDate,
                            FirstNote = prev.Note,
                            SecondNote = curr.Note,
                            DepartmentName = prev.TreatmentRecordDetail?.Room?.Department?.Name ?? "",
                            RoomName = prev.TreatmentRecordDetail?.Room?.Name ?? "",
                            EmployeeName = (prev.EmployeeId.HasValue && employeeDict.ContainsKey(prev.EmployeeId.Value)) ? employeeDict[prev.EmployeeId.Value] : ""
                        });
                    }
                }
            }

            warningPatients = warningPatients
                .OrderBy(x => x.PatientName)
                .ThenBy(x => x.FirstAbsenceDate)
                .ToList();

            await _viewBagHelper.BaseViewBag(ViewData);
            return View(warningPatients);
        }
    }
}
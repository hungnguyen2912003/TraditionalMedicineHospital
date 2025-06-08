using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.NhanVien.Models.ViewModels;
using Project.Helpers;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using Project.Services.Features;

namespace Project.Areas.NhanVien.Controllers
{
    [Area("NhanVien")]
    [Authorize(Roles = "NhanVienHanhChinh")]
    [Route("canh-bao-benh-nhan")]
    public class WarningPatientsController : Controller
    {
        private readonly ITreatmentTrackingRepository _treatmentTrackingRepository;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly IMapper _mapper;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserRepository _userRepository;
        private readonly EmailService _emailService;
        private readonly IRoomRepository _roomRepository;

        public WarningPatientsController
        (
            ITreatmentTrackingRepository treatmentTrackingRepository,
            ViewBagHelper viewBagHelper,
            IMapper mapper,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository,
            EmailService emailService,
            IRoomRepository roomRepository
        )
        {
            _treatmentTrackingRepository = treatmentTrackingRepository;
            _viewBagHelper = viewBagHelper;
            _mapper = mapper;
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _roomRepository = roomRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Lấy tất cả tracking
            var allTrackings = await _treatmentTrackingRepository.GetAllAdvancedAsync();

            // Lấy mã nhân viên hiện tại từ cookie AuthToken
            string? currentEmployeeCode = null;
            string? currentDepartmentName = null;
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
                        currentDepartmentName = user.Employee.Room?.Department?.Name;
                        ViewBag.CurrentEmployeeCode = currentEmployeeCode;
                        ViewBag.CurrentDepartmentName = currentDepartmentName;
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
                    t.TreatmentRecordDetailId
                })
                .Select(g => new
                {
                    g.Key.PatientId,
                    g.Key.TreatmentRecordDetailId,
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
                int i = 1;
                while (i < ordered.Count)
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
                            DepName = prev.TreatmentRecordDetail?.Room?.Department?.Name ?? "",
                            RoomName = prev.TreatmentRecordDetail?.Room?.Name ?? "",
                            EmployeeName = prev.EmployeeId.HasValue && employeeDict.ContainsKey(prev.EmployeeId.Value) ? employeeDict[prev.EmployeeId.Value] : ""
                        });
                        // Bỏ qua các ngày tiếp theo trong chuỗi liên tiếp "Không điều trị"
                        while (i + 1 < ordered.Count &&
                               (ordered[i + 1].TrackingDate.Date - ordered[i].TrackingDate.Date).TotalDays == 1 &&
                               ordered[i].Status == TrackingStatus.KhongDieuTri &&
                               ordered[i + 1].Status == TrackingStatus.KhongDieuTri)
                        {
                            i++;
                        }
                    }
                    i++;
                }
            }

            if (!string.IsNullOrEmpty(currentDepartmentName))
            {
                var normalizedDept = currentDepartmentName.Trim().ToLower();
                warningPatients = warningPatients
                    .Where(x => !string.IsNullOrEmpty(x.DepName) && x.DepName.Trim().ToLower() == normalizedDept)
                    .OrderBy(x => x.PatientName)
                    .ThenBy(x => x.FirstAbsenceDate)
                    .ToList();
            }
            else
            {
                warningPatients = warningPatients
                    .OrderBy(x => x.PatientName)
                    .ThenBy(x => x.FirstAbsenceDate)
                    .ToList();
            }

            // Sau khi xác định currentDepartmentName
            var allRooms = await _roomRepository.GetAllAsync();
            List<Room> filterRooms;
            if (!string.IsNullOrEmpty(currentDepartmentName))
            {
                var normalizedDept = currentDepartmentName.Trim().ToLower();
                filterRooms = allRooms
                    .Where(r => r.Department != null && r.Department.Name != null && r.Department.Name.Trim().ToLower() == normalizedDept)
                    .ToList();
            }
            else
            {
                filterRooms = allRooms.ToList();
            }
            ViewBag.FilterRooms = filterRooms;

            await _viewBagHelper.BaseViewBag(ViewData);
            return View(warningPatients);
        }
    }
}
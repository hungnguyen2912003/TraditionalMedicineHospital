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
        private readonly IWarningSentRepository _warningSentRepository;

        public WarningPatientsController
        (
            ITreatmentTrackingRepository treatmentTrackingRepository,
            ViewBagHelper viewBagHelper,
            IMapper mapper,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository,
            EmailService emailService,
            IRoomRepository roomRepository,
            IWarningSentRepository warningSentRepository
        )
        {
            _treatmentTrackingRepository = treatmentTrackingRepository;
            _viewBagHelper = viewBagHelper;
            _mapper = mapper;
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _roomRepository = roomRepository;
            _warningSentRepository = warningSentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1. Lấy thông tin nhân viên hiện tại từ token
            string? currentEmployeeCode = null;
            string? currentDepartmentName = null;
            var token = Request.Cookies["AuthToken"];
            if (!string.IsNullOrEmpty(token))
            {
                // Giải mã token để lấy username và role
                var (username, role) = _viewBagHelper._jwtManager.GetClaimsFromToken(token);
                if (!string.IsNullOrEmpty(username))
                {
                    // Lấy thông tin user từ database
                    var user = await _userRepository.GetByUsernameAsync(username);
                    if (user != null && user.Employee != null)
                    {
                        // Lưu thông tin nhân viên vào ViewBag để sử dụng trong view
                        currentEmployeeCode = user.Employee.Code;
                        currentDepartmentName = user.Employee.Room?.Department?.Name;
                        ViewBag.CurrentEmployeeCode = currentEmployeeCode;
                        ViewBag.CurrentDepartmentName = currentDepartmentName;
                        ViewBag.CurrentRole = user.Role.ToString();
                    }
                }
            }

            // 2. Lấy danh sách tracking dựa trên khoa của nhân viên
            List<TreatmentTracking> allTrackings;
            if (!string.IsNullOrEmpty(currentDepartmentName))
            {
                // Nếu nhân viên thuộc khoa nào đó, chỉ lấy tracking của khoa đó
                allTrackings = (await _treatmentTrackingRepository.GetByDepartmentAsync(currentDepartmentName)).ToList();
            }
            else
            {
                // Nếu không thuộc khoa nào, lấy tất cả tracking
                allTrackings = (await _treatmentTrackingRepository.GetAllAdvancedAsync()).ToList();
            }

            // 3. Nhóm các tracking theo bệnh nhân và từng đợt điều trị
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
                    PatientPhone = g.First().TreatmentRecordDetail!.TreatmentRecord!.Patient!.PhoneNumber,
                    Trackings = g.OrderBy(t => t.TrackingDate).ToList()
                })
                .ToList();

            // 4. Lấy thông tin nhân viên từ các tracking
            var employeeIds = patientGroups
                .SelectMany(p => p.Trackings)
                .Where(t => t.EmployeeId.HasValue)
                .Select(t => t.EmployeeId!.Value)
                .Distinct()
                .ToList();
            var employeeDict = new Dictionary<Guid, string>();
            if (employeeIds.Any())
            {
                // Tạo dictionary để map EmployeeId với tên nhân viên
                var employees = await _employeeRepository.GetByIdsAsync(employeeIds);
                employeeDict = employees.ToDictionary(e => e.Id, e => e.Name);
            }

            // 5. Lấy danh sách các cảnh báo đã gửi
            var warningSents = (await _warningSentRepository.GetAllAsync()).ToList();

            // 6. Tìm các bệnh nhân có dấu hiệu vắng mặt liên tiếp
            var warningPatients = new List<WarningPatientViewModel>();
            foreach (var p in patientGroups)
            {
                var ordered = p.Trackings;
                int i = 1;
                while (i < ordered.Count)
                {
                    // Lấy tracking trước và sau đó
                    var prev = ordered[i - 1];
                    var curr = ordered[i];
                    // Tính số ngày giữa 2 lần vắng mặt
                    var daysDiff = (curr.TrackingDate.Date - prev.TrackingDate.Date).TotalDays;

                    // Kiểm tra nếu bệnh nhân vắng mặt 2 ngày liên tiếp
                    if (daysDiff == 1 &&
                        prev.Status == TrackingStatus.KhongDieuTri &&
                        curr.Status == TrackingStatus.KhongDieuTri)
                    {
                        // Tạo đối tượng cảnh báo mới
                        warningPatients.Add(new WarningPatientViewModel
                        {
                            PatientId = p.PatientId,
                            TreatmentRecordDetailId = p.TreatmentRecordDetailId ?? Guid.Empty,
                            PatientName = p.PatientName,
                            PatientEmail = p.PatientEmail,
                            PatientPhone = p.PatientPhone,
                            FirstAbsenceDate = prev.TrackingDate,
                            SecondAbsenceDate = curr.TrackingDate,
                            DepName = prev.TreatmentRecordDetail?.Room?.Department?.Name ?? "",
                            RoomName = prev.TreatmentRecordDetail?.Room?.Name ?? "",
                            EmployeeName = prev.EmployeeId.HasValue && employeeDict.ContainsKey(prev.EmployeeId.Value) ? employeeDict[prev.EmployeeId.Value] : "",
                            // Kiểm tra xem đã gửi cảnh báo qua email chưa
                            mailSent = warningSents.Any(x => x.PatientId == p.PatientId &&
                                x.TreatmentRecordDetailId == (p.TreatmentRecordDetailId ?? Guid.Empty) &&
                                x.FirstAbsenceDate.Date == prev.TrackingDate.Date &&
                                x.Type == WarningSentType.Mail),
                            // Kiểm tra xem đã gửi cảnh báo qua SMS chưa
                            smsSent = warningSents.Any(x => x.PatientId == p.PatientId &&
                                x.TreatmentRecordDetailId == (p.TreatmentRecordDetailId ?? Guid.Empty) &&
                                x.FirstAbsenceDate.Date == prev.TrackingDate.Date &&
                                x.Type == WarningSentType.Sms)
                        });

                        // Bỏ qua các ngày vắng mặt liên tiếp tiếp theo
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

            // 7. Lọc và sắp xếp danh sách cảnh báo theo khoa
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

            // 8. Lấy danh sách phòng để lọc
            var allRooms = await _roomRepository.GetAllAsync();
            List<Room> filterRooms;
            if (!string.IsNullOrEmpty(currentDepartmentName))
            {
                // Lọc phòng theo khoa của nhân viên
                var normalizedDept = currentDepartmentName.Trim().ToLower();
                filterRooms = allRooms
                    .Where(r => r.Department != null && r.Department.Name != null &&
                           r.Department.Name.Trim().ToLower() == normalizedDept)
                    .ToList();
            }
            else
            {
                filterRooms = allRooms.ToList();
            }
            ViewBag.FilterRooms = filterRooms;

            // 9. Cập nhật ViewBag và trả về view
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(warningPatients);
        }
    }
}
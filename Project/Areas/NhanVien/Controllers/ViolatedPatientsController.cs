using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Helpers;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using Project.Models.Enums;
using Project.Areas.NhanVien.Models.ViewModels;
using Project.Areas.Admin.Models.Entities;

namespace Project.Areas.NhanVien.Controllers
{
    [Area("NhanVien")]
    [Authorize(Roles = "NhanVienHanhChinh")]
    [Route("benh-nhan-vi-pham")]
    public class ViolatedPatientsController : Controller
    {
        private readonly ITreatmentTrackingRepository _treatmentTrackingRepository;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly IMapper _mapper;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserRepository _userRepository;
        private readonly EmailService _emailService;
        private readonly IRoomRepository _roomRepository;
        private readonly IWarningSentRepository _warningSentRepository;
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;

        public ViolatedPatientsController
        (
            ITreatmentTrackingRepository treatmentTrackingRepository,
            ViewBagHelper viewBagHelper,
            IMapper mapper,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository,
            EmailService emailService,
            IRoomRepository roomRepository,
            IWarningSentRepository warningSentRepository,
            ITreatmentRecordRepository treatmentRecordRepository
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
            _treatmentRecordRepository = treatmentRecordRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1. Lấy thông tin nhân viên hiện tại từ cookie AuthToken
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

            // 2. Lấy danh sách tracking theo khoa của nhân viên hiện tại
            List<TreatmentTracking> allTrackings = (await _treatmentTrackingRepository.GetByDepartmentAsync(currentDepartmentName!)).ToList();

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

            // 4. Lấy tất cả EmployeeId từ các tracking để map sang tên nhân viên
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

            // 5. Lọc ra các bệnh nhân có 3 ngày vắng liên tiếp
            var violatedPatients = new List<ViolatedPatientViewModel>();
            foreach (var p in patientGroups)
            {
                var ordered = p.Trackings;
                int i = 2; // Bắt đầu từ index 2 để kiểm tra 3 ngày liên tiếp
                while (i < ordered.Count)
                {
                    // Lấy 3 ngày liên tiếp
                    var prev2 = ordered[i - 2];
                    var prev1 = ordered[i - 1];
                    var curr = ordered[i];

                    // Tính số ngày giữa các lần tracking
                    var daysDiff1 = (prev1.TrackingDate.Date - prev2.TrackingDate.Date).TotalDays;
                    var daysDiff2 = (curr.TrackingDate.Date - prev1.TrackingDate.Date).TotalDays;

                    // Kiểm tra nếu cả 3 ngày đều liên tiếp và đều không điều trị, đồng thời phiếu điều trị đang hoạt động
                    if (daysDiff1 == 1 && daysDiff2 == 1 &&
                        prev2.Status == TrackingStatus.KhongDieuTri &&
                        prev1.Status == TrackingStatus.KhongDieuTri &&
                        curr.Status == TrackingStatus.KhongDieuTri &&
                        prev2.TreatmentRecordDetail?.TreatmentRecord?.Status == TreatmentStatus.DangDieuTri)
                    {
                        // Thêm vào danh sách bệnh nhân vi phạm
                        violatedPatients.Add(new ViolatedPatientViewModel
                        {
                            PatientId = p.PatientId,
                            TreatmentRecordDetailId = p.TreatmentRecordDetailId ?? Guid.Empty,
                            PatientName = p.PatientName,
                            PatientEmail = p.PatientEmail,
                            PatientPhone = p.PatientPhone,
                            FirstAbsenceDate = prev2.TrackingDate,
                            SecondAbsenceDate = prev1.TrackingDate,
                            ThirdAbsenceDate = curr.TrackingDate,
                            DepName = prev2.TreatmentRecordDetail?.Room?.Department?.Name ?? "",
                            RoomName = prev2.TreatmentRecordDetail?.Room?.Name ?? "",
                            EmployeeName = prev2.EmployeeId.HasValue && employeeDict.ContainsKey(prev2.EmployeeId.Value) ? employeeDict[prev2.EmployeeId.Value] : "",
                            IsViolated = prev2.TreatmentRecordDetail?.TreatmentRecord?.IsViolated ?? false,
                            TreatmentRecordId = prev2.TreatmentRecordDetail?.TreatmentRecord?.Id ?? Guid.Empty
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

            // 6. Lọc lại danh sách bệnh nhân vi phạm theo phòng ban nếu có
            if (!string.IsNullOrEmpty(currentDepartmentName))
            {
                var normalizedDept = currentDepartmentName.Trim().ToLower();
                violatedPatients = violatedPatients
                    .Where(x => !string.IsNullOrEmpty(x.DepName) && x.DepName.Trim().ToLower() == normalizedDept)
                    .OrderBy(x => x.PatientName)
                    .ThenBy(x => x.FirstAbsenceDate)
                    .ToList();
            }
            else
            {
                violatedPatients = violatedPatients
                    .OrderBy(x => x.PatientName)
                    .ThenBy(x => x.FirstAbsenceDate)
                    .ToList();
            }

            // 7. Lấy danh sách phòng để filter trên giao diện
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

            // 8. Cập nhật ViewBag và trả về view với danh sách bệnh nhân vi phạm
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(violatedPatients);
        }

        [HttpPost("SetPreSuspend")]
        public async Task<IActionResult> SetPreSuspend(Guid treatmentRecordId)
        {
            var record = await _treatmentRecordRepository.GetByIdAsync(treatmentRecordId);
            if (record == null)
                return NotFound();

            record.IsViolated = true;
            await _treatmentRecordRepository.UpdateAsync(record);

            return Ok(new { success = true });
        }
    }
}

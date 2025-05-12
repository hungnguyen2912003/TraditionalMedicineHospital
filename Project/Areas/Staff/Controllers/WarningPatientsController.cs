using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Repositories.Interfaces;
using AutoMapper;
using Project.Areas.Staff.Models.ViewModels;
using Project.Helpers;
using Project.Models.Enums;

namespace Project.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Bacsi, Yta")]
    public class WarningPatientsController : Controller
    {
        private readonly ITreatmentTrackingRepository _treatmentTrackingRepository;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly IMapper _mapper;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserRepository _userRepository;

        public WarningPatientsController
        (
            ITreatmentTrackingRepository treatmentTrackingRepository,
            ViewBagHelper viewBagHelper,
            IMapper mapper,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository
        )
        {
            _treatmentTrackingRepository = treatmentTrackingRepository;
            _viewBagHelper = viewBagHelper;
            _mapper = mapper;
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy tất cả tracking
            var allTrackings = await _treatmentTrackingRepository.GetAllAdvancedAsync();
            var activeTrackings = allTrackings.Where(x => x.IsActive).ToList();

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

            // Lọc chỉ các bản ghi do nhân viên hiện tại thực hiện
            if (!string.IsNullOrEmpty(currentEmployeeCode))
            {
                activeTrackings = activeTrackings.Where(x => x.CreatedBy == currentEmployeeCode).ToList();
            }

            // Nhóm các tracking theo bệnh nhân
            var patientGroups = activeTrackings
                .Where(t => t.TreatmentRecordDetail?.TreatmentRecord?.Patient != null)
                .GroupBy(t => t.TreatmentRecordDetail!.TreatmentRecord!.Patient!.Id)
                .Select(g => new
                {
                    PatientId = g.Key,
                    PatientName = g.First().TreatmentRecordDetail!.TreatmentRecord!.Patient!.Name,
                    PatientEmail = g.First().TreatmentRecordDetail!.TreatmentRecord!.Patient!.EmailAddress,
                    Trackings = g.OrderByDescending(t => t.TrackingDate).ToList()
                })
                .ToList();

            // Lọc ra các bệnh nhân có 2 lần vắng mặt liên tiếp
            var warningPatients = patientGroups
                .Where(p => p.Trackings.Count >= 2)
                .Select(p => new
                {
                    p.PatientId,
                    p.PatientName,
                    p.PatientEmail,
                    RecentTrackings = p.Trackings.Take(2).ToList()
                })
                .Where(p =>
                {
                    var firstTracking = p.RecentTrackings[1];
                    var secondTracking = p.RecentTrackings[0];

                    // Kiểm tra xem 2 ngày có liên tiếp không và đều là trạng thái không điều trị
                    var daysDiff = (secondTracking.TrackingDate.Date - firstTracking.TrackingDate.Date).TotalDays;
                    return daysDiff == 1 &&
                           firstTracking.Status == TrackingStatus.KhongDieuTri &&
                           secondTracking.Status == TrackingStatus.KhongDieuTri;
                })
                .Select(p => new WarningPatientViewModel
                {
                    PatientId = p.PatientId,
                    PatientName = p.PatientName,
                    PatientEmail = p.PatientEmail,
                    FirstAbsenceDate = p.RecentTrackings[1].TrackingDate,
                    SecondAbsenceDate = p.RecentTrackings[0].TrackingDate,
                    FirstNote = p.RecentTrackings[1].Note,
                    SecondNote = p.RecentTrackings[0].Note
                })
                .ToList();

            await _viewBagHelper.BaseViewBag(ViewData);
            return View(warningPatients);
        }
    }
} 
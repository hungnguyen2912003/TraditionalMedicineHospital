using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Threading.Tasks;
using Project.Services.Features;
using Project.Areas.NhanVien.Models.DTOs;
using Project.Repositories.Interfaces;
using Project.Models.Enums;
using System.Linq;
using Project.Areas.Admin.Models.Entities;
using Project.Helpers;

namespace Project.Areas.NhanVien.Controllers.api
{
    [Area("NhanVien")]
    [Route("api/NhanVien/[controller]/[action]")]
    [ApiController]
    public class WarningHandlesController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly SmsService _smsService;
        private readonly ITreatmentTrackingRepository _treatmentTrackingRepository;
        private readonly CodeGeneratorHelper _codeGenerator;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IWarningSentRepository _warningMailSentRepository;
        private readonly JwtManager _jwtManager;
        private readonly IUserRepository _userRepository;
        public WarningHandlesController
        (
            EmailService emailService,
            SmsService smsService,
            ITreatmentTrackingRepository treatmentTrackingRepository,
            IEmployeeRepository employeeRepository,
            IWarningSentRepository warningMailSentRepository,
            CodeGeneratorHelper codeGenerator,
            JwtManager jwtManager,
            IUserRepository userRepository,
            IWarningSentRepository warningSmsSentRepository
        )
        {
            _emailService = emailService;
            _smsService = smsService;
            _treatmentTrackingRepository = treatmentTrackingRepository;
            _employeeRepository = employeeRepository;
            _warningMailSentRepository = warningMailSentRepository;
            _codeGenerator = codeGenerator;
            _jwtManager = jwtManager;
            _userRepository = userRepository;
        }

        private string NormalizePhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return phone;
            phone = phone.Trim();
            if (phone.StartsWith("0") && phone.Length == 10)
                return "+84" + phone.Substring(1);
            if (phone.StartsWith("+84"))
                return phone;
            return phone;
        }

        [HttpPost]
        public async Task<IActionResult> SendMailToPatient([FromBody] SendMailByWarningRequest req)
        {
            // Get user info from token
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Người dùng chưa đăng nhập");
            }

            var (username, role) = _jwtManager.GetClaimsFromToken(token);
            if (string.IsNullOrEmpty(username))
            {
                Response.Cookies.Delete("AuthToken");
                return BadRequest("Token không hợp lệ.");
            }

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || user.Employee == null)
            {
                return BadRequest("Người dùng không hợp lệ");
            }

            // Lấy tất cả tracking
            var allTrackings = await _treatmentTrackingRepository.GetAllAdvancedAsync();

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

            // Tìm đúng group
            var group = patientGroups.FirstOrDefault(x => x.PatientId == req.PatientId && x.TreatmentRecordDetailId == req.TreatmentRecordDetailId);
            if (group == null)
                return NotFound("Không tìm thấy bệnh nhân hoặc đợt điều trị phù hợp.");

            // Tìm đúng cặp ngày cảnh báo đầu tiên
            var ordered = group.Trackings;
            int i = 1;
            DateTime? prevDate = null, lastDate = null;
            TreatmentTracking prev = null!, curr = null!;
            while (i < ordered.Count)
            {
                prev = ordered[i - 1];
                curr = ordered[i];
                var daysDiff = (curr.TrackingDate.Date - prev.TrackingDate.Date).TotalDays;
                if (daysDiff == 1 &&
                    prev.Status == TrackingStatus.KhongDieuTri &&
                    curr.Status == TrackingStatus.KhongDieuTri)
                {
                    prevDate = prev.TrackingDate;
                    lastDate = curr.TrackingDate;
                    break;
                }
                i++;
            }
            if (prevDate == null || lastDate == null)
                return BadRequest("Không tìm thấy cặp ngày cảnh báo phù hợp.");

            // Lấy thông tin bổ sung
            var employeeName = prev.EmployeeId.HasValue
                ? (await _employeeRepository.GetByIdAsync(prev.EmployeeId.Value))?.Name ?? ""
                : "";
            var treatmentMethod = prev.TreatmentRecordDetail?.Room?.TreatmentMethod?.Name ?? "";
            var roomName = prev.TreatmentRecordDetail?.Room?.Name ?? "";
            var departmentName = prev.TreatmentRecordDetail?.Room?.Department?.Name ?? "";

            if (string.IsNullOrEmpty(group.PatientEmail))
                return BadRequest("Bệnh nhân không có email.");

            var subject = "Nhắc nhở điều trị - Bệnh viện Y học cổ truyền Nha Trang";
            var body = $@"
                <h2>Xin chào {group.PatientName},</h2>
                <p>Hệ thống ghi nhận bạn đã vắng mặt trong 2 ngày liên tiếp: Ngày <b>{prevDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}</b> và Ngày <b>{lastDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}</b> bởi nhân viên <b>{employeeName}</b> thực hiện Phương pháp <b>{treatmentMethod}</b> thuộc phòng <b>{roomName}</b> tại Khoa <b>{departmentName}</b>.</p>
                <p>Để đảm bảo hiệu quả điều trị, vui lòng sắp xếp thời gian đến bệnh viện để tiếp tục điều trị.</p>
                <p>Nếu bạn có lý do đặc biệt, vui lòng liên hệ với bác sĩ điều trị của bạn.</p>
                <p>Trân trọng,<br/>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>";

            try
            {
                await _emailService.SendEmailAsync(group.PatientEmail, subject, body);

                var warningMailSent = new WarningSent
                {
                    Id = Guid.NewGuid(),
                    PatientId = req.PatientId,
                    Code = await _codeGenerator.GenerateUniqueCodeAsync(_warningMailSentRepository),
                    TreatmentRecordDetailId = req.TreatmentRecordDetailId,
                    FirstAbsenceDate = prevDate.Value,
                    SentAt = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    Type = WarningSentType.Mail,
                    CreatedBy = user.Employee.Code,
                    UpdatedBy = user.Employee.Code
                };
                await _warningMailSentRepository.CreateAsync(warningMailSent);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest($"Gửi email thất bại: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PreviewMailToPatient([FromQuery] Guid patientId, [FromQuery] Guid treatmentRecordDetailId, [FromQuery] DateTime firstAbsenceDate)
        {
            // Lấy tất cả tracking
            var allTrackings = await _treatmentTrackingRepository.GetAllAdvancedAsync();

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
                    Trackings = g.OrderBy(t => t.TrackingDate).ToList(),
                    TreatmentRecordDetailCode = g.OrderBy(t => t.TrackingDate).FirstOrDefault()?.TreatmentRecordDetail?.Code ?? g.Key.TreatmentRecordDetailId.ToString()
                })
                .ToList();

            // Tìm đúng group
            var group = patientGroups.FirstOrDefault(x => x.PatientId == patientId && x.TreatmentRecordDetailId == treatmentRecordDetailId);
            if (group == null)
                return NotFound("Không tìm thấy bệnh nhân hoặc đợt điều trị phù hợp.");

            // Tìm đúng cặp ngày cảnh báo theo firstAbsenceDate
            var ordered = group.Trackings;
            TreatmentTracking prev = null!, curr = null!;
            DateTime? prevDate = null, lastDate = null;
            for (int i = 1; i < ordered.Count; i++)
            {
                prev = ordered[i - 1];
                curr = ordered[i];
                var daysDiff = (curr.TrackingDate.Date - prev.TrackingDate.Date).TotalDays;
                if (daysDiff == 1 &&
                    prev.Status == TrackingStatus.KhongDieuTri &&
                    curr.Status == TrackingStatus.KhongDieuTri &&
                    prev.TrackingDate.Date == firstAbsenceDate.Date)
                {
                    prevDate = prev.TrackingDate;
                    lastDate = curr.TrackingDate;
                    break;
                }
            }
            if (prevDate == null || lastDate == null)
                return BadRequest("Không tìm thấy cặp ngày cảnh báo phù hợp.");

            var employeeName = prev.EmployeeId.HasValue
                ? (await _employeeRepository.GetByIdAsync(prev.EmployeeId.Value))?.Name ?? ""
                : "";
            var treatmentMethod = prev.TreatmentRecordDetail?.Room?.TreatmentMethod?.Name ?? "";
            var roomName = prev.TreatmentRecordDetail?.Room?.Name ?? "";
            var departmentName = prev.TreatmentRecordDetail?.Room?.Department?.Name ?? "";

            if (string.IsNullOrEmpty(group.PatientEmail))
                return BadRequest("Bệnh nhân không có email.");

            var subject = "Nhắc nhở điều trị - Bệnh viện Y học cổ truyền Nha Trang";
            var body = $@"
                <h2>Xin chào {group.PatientName},</h2>
                <p>Hệ thống ghi nhận bạn đã vắng mặt trong 2 ngày liên tiếp: Ngày <b>{prevDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}</b> và Ngày <b>{lastDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}</b> bởi nhân viên <b>{employeeName}</b> thực hiện Phương pháp <b>{treatmentMethod}</b> thuộc phòng <b>{roomName}</b> tại Khoa <b>{departmentName}</b>.</p>
                <p>Để đảm bảo hiệu quả điều trị, vui lòng sắp xếp thời gian đến bệnh viện để tiếp tục điều trị.</p>
                <p>Nếu bạn có lý do đặc biệt, vui lòng liên hệ với bác sĩ điều trị của bạn.</p>
                <p>Trân trọng,<br/>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>";

            return Ok(new
            {
                subject,
                body,
                patientName = group.PatientName,
                patientEmail = group.PatientEmail,
                treatmentRecordDetailId = group.TreatmentRecordDetailId,
                treatmentRecordDetailCode = group.TreatmentRecordDetailCode,
                prevAbsenceDate = prevDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                lastAbsenceDate = lastDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
            });
        }

        [HttpGet]
        public async Task<IActionResult> PreviewSmsToPatient([FromQuery] Guid patientId, [FromQuery] Guid treatmentRecordDetailId, [FromQuery] DateTime firstAbsenceDate)
        {
            // Lấy tất cả tracking
            var allTrackings = await _treatmentTrackingRepository.GetAllAdvancedAsync();

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
                    PatientPhone = g.First().TreatmentRecordDetail!.TreatmentRecord!.Patient!.PhoneNumber,
                    Trackings = g.OrderBy(t => t.TrackingDate).ToList(),
                    TreatmentRecordDetailCode = g.OrderBy(t => t.TrackingDate).FirstOrDefault()?.TreatmentRecordDetail?.Code ?? g.Key.TreatmentRecordDetailId.ToString()
                })
                .ToList();

            // Tìm đúng group
            var group = patientGroups.FirstOrDefault(x => x.PatientId == patientId && x.TreatmentRecordDetailId == treatmentRecordDetailId);
            if (group == null)
                return NotFound("Không tìm thấy bệnh nhân hoặc đợt điều trị phù hợp.");

            // Tìm đúng cặp ngày cảnh báo theo firstAbsenceDate
            var ordered = group.Trackings;
            TreatmentTracking prev = null!, curr = null!;
            DateTime? prevDate = null, lastDate = null;
            for (int i = 1; i < ordered.Count; i++)
            {
                prev = ordered[i - 1];
                curr = ordered[i];
                var daysDiff = (curr.TrackingDate.Date - prev.TrackingDate.Date).TotalDays;
                if (daysDiff == 1 &&
                    prev.Status == TrackingStatus.KhongDieuTri &&
                    curr.Status == TrackingStatus.KhongDieuTri &&
                    prev.TrackingDate.Date == firstAbsenceDate.Date)
                {
                    prevDate = prev.TrackingDate;
                    lastDate = curr.TrackingDate;
                    break;
                }
            }
            if (prevDate == null || lastDate == null)
                return BadRequest("Không tìm thấy cặp ngày cảnh báo phù hợp.");

            var employeeName = prev.EmployeeId.HasValue
                ? (await _employeeRepository.GetByIdAsync(prev.EmployeeId.Value))?.Name ?? ""
                : "";
            var treatmentMethod = prev.TreatmentRecordDetail?.Room?.TreatmentMethod?.Name ?? "";
            var roomName = prev.TreatmentRecordDetail?.Room?.Name ?? "";
            var departmentName = prev.TreatmentRecordDetail?.Room?.Department?.Name ?? "";

            if (string.IsNullOrEmpty(group.PatientPhone))
                return BadRequest("Bệnh nhân không có số điện thoại.");

            var message = $@"
                Xin chào {group.PatientName}, hệ thống ghi nhận bạn đã vắng mặt trong 2 ngày liên tiếp: Ngày {prevDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} và Ngày {lastDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} bởi nhân viên {employeeName} thực hiện Phương pháp {treatmentMethod} thuộc phòng {roomName} tại Khoa {departmentName}. Để đảm bảo hiệu quả điều trị, vui lòng sắp xếp thời gian đến bệnh viện để tiếp tục điều trị. Nếu bạn có lý do đặc biệt, vui lòng liên hệ với bác sĩ điều trị của bạn. Trân trọng, Bệnh viện Y học cổ truyền Nha Trang";

            return Ok(new
            {
                message,
                patientName = group.PatientName,
                patientPhone = group.PatientPhone,
                treatmentRecordDetailId = group.TreatmentRecordDetailId,
                treatmentRecordDetailCode = group.TreatmentRecordDetailCode,
                prevAbsenceDate = prevDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                lastAbsenceDate = lastDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
            });
        }

        [HttpPost]
        public async Task<IActionResult> SendSmsToPatient([FromBody] SendSmsByWarningRequest req)
        {
            // Get user info from token
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Người dùng chưa đăng nhập");
            }

            var (username, role) = _jwtManager.GetClaimsFromToken(token);
            if (string.IsNullOrEmpty(username))
            {
                Response.Cookies.Delete("AuthToken");
                return BadRequest("Token không hợp lệ.");
            }

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || user.Employee == null)
            {
                return BadRequest("Người dùng không hợp lệ");
            }

            // Lấy tất cả tracking
            var allTrackings = await _treatmentTrackingRepository.GetAllAdvancedAsync();

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
                    PatientPhone = g.First().TreatmentRecordDetail!.TreatmentRecord!.Patient!.PhoneNumber,
                    Trackings = g.OrderBy(t => t.TrackingDate).ToList()
                })
                .ToList();

            // Tìm đúng group
            var group = patientGroups.FirstOrDefault(x => x.PatientId == req.PatientId && x.TreatmentRecordDetailId == req.TreatmentRecordDetailId);
            if (group == null)
                return NotFound("Không tìm thấy bệnh nhân hoặc đợt điều trị phù hợp.");

            // Tìm đúng cặp ngày cảnh báo đầu tiên
            var ordered = group.Trackings;
            int i = 1;
            DateTime? prevDate = null, lastDate = null;
            TreatmentTracking prev = null!, curr = null!;
            while (i < ordered.Count)
            {
                prev = ordered[i - 1];
                curr = ordered[i];
                var daysDiff = (curr.TrackingDate.Date - prev.TrackingDate.Date).TotalDays;
                if (daysDiff == 1 &&
                    prev.Status == TrackingStatus.KhongDieuTri &&
                    curr.Status == TrackingStatus.KhongDieuTri)
                {
                    prevDate = prev.TrackingDate;
                    lastDate = curr.TrackingDate;
                    break;
                }
                i++;
            }
            if (prevDate == null || lastDate == null)
                return BadRequest("Không tìm thấy cặp ngày cảnh báo phù hợp.");

            var employeeName = prev.EmployeeId.HasValue
                ? (await _employeeRepository.GetByIdAsync(prev.EmployeeId.Value))?.Name ?? ""
                : "";
            var treatmentMethod = prev.TreatmentRecordDetail?.Room?.TreatmentMethod?.Name ?? "";
            var roomName = prev.TreatmentRecordDetail?.Room?.Name ?? "";
            var departmentName = prev.TreatmentRecordDetail?.Room?.Department?.Name ?? "";

            if (string.IsNullOrEmpty(group.PatientPhone))
                return BadRequest("Bệnh nhân không có số điện thoại.");

            var message = $@"
                Xin chào {group.PatientName}, hệ thống ghi nhận bạn đã vắng mặt trong 2 ngày liên tiếp: Ngày {prevDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} và Ngày {lastDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} bởi nhân viên {employeeName} thực hiện Phương pháp {treatmentMethod} thuộc phòng {roomName} tại Khoa {departmentName}. Để đảm bảo hiệu quả điều trị, vui lòng sắp xếp thời gian đến bệnh viện để tiếp tục điều trị. Nếu bạn có lý do đặc biệt, vui lòng liên hệ với bác sĩ điều trị của bạn. Trân trọng, Bệnh viện Y học cổ truyền Nha Trang";

            var normalizedPhone = NormalizePhoneNumber(group.PatientPhone);
            try
            {
                _smsService.SendSms(normalizedPhone, message);

                var warningSmsSent = new WarningSent
                {
                    Id = Guid.NewGuid(),
                    PatientId = req.PatientId,
                    Code = await _codeGenerator.GenerateUniqueCodeAsync(_warningMailSentRepository),
                    TreatmentRecordDetailId = req.TreatmentRecordDetailId,
                    FirstAbsenceDate = prevDate.Value,
                    SentAt = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    Type = WarningSentType.Sms,
                    CreatedBy = user.Employee.Code,
                    UpdatedBy = user.Employee.Code
                };
                await _warningMailSentRepository.CreateAsync(warningSmsSent);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest($"Gửi SMS thất bại: {ex.Message}");
            }
        }
    }
}
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
        private readonly ITreatmentTrackingRepository _treatmentTrackingRepository;
        private readonly CodeGeneratorHelper _codeGenerator;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IWarningMailSentRepository _warningMailSentRepository;
        private readonly JwtManager _jwtManager;
        private readonly IUserRepository _userRepository;
        public WarningHandlesController(EmailService emailService, ITreatmentTrackingRepository treatmentTrackingRepository, IEmployeeRepository employeeRepository, IWarningMailSentRepository warningMailSentRepository, CodeGeneratorHelper codeGenerator, JwtManager jwtManager, IUserRepository userRepository)
        {
            _emailService = emailService;
            _treatmentTrackingRepository = treatmentTrackingRepository;
            _employeeRepository = employeeRepository;
            _warningMailSentRepository = warningMailSentRepository;
            _codeGenerator = codeGenerator;
            _jwtManager = jwtManager;
            _userRepository = userRepository;
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
            while (i < ordered.Count)
            {
                var prev = ordered[i - 1];
                var curr = ordered[i];
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

            if (string.IsNullOrEmpty(group.PatientEmail))
                return BadRequest("Bệnh nhân không có email.");

            var subject = "Nhắc nhở điều trị - Bệnh viện Y học cổ truyền Nha Trang";
            var body = $@"
                <h2>Xin chào {group.PatientName},</h2>
                <p>Hệ thống ghi nhận bạn đã vắng mặt trong 2 ngày liên tiếp: Ngày {prevDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} và Ngày {lastDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}.</p>
                <p>Để đảm bảo hiệu quả điều trị, vui lòng sắp xếp thời gian đến bệnh viện để tiếp tục điều trị.</p>
                <p>Nếu bạn có lý do đặc biệt, vui lòng liên hệ với bác sĩ điều trị của bạn.</p>
                <p>Trân trọng,<br/>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>";

            await _emailService.SendEmailAsync(group.PatientEmail, subject, body);

            var warningMailSent = new WarningMailSent
            {
                Id = Guid.NewGuid(),
                PatientId = req.PatientId,
                Code = await _codeGenerator.GenerateUniqueCodeAsync(_warningMailSentRepository),
                TreatmentRecordDetailId = req.TreatmentRecordDetailId,
                FirstAbsenceDate = prevDate.Value,
                SentAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                CreatedBy = user.Employee.Code,
                UpdatedBy = user.Employee.Code
            };
            await _warningMailSentRepository.CreateAsync(warningMailSent);

            return Ok(new { success = true });
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
            DateTime? prevDate = null, lastDate = null;
            for (int i = 1; i < ordered.Count; i++)
            {
                var prev = ordered[i - 1];
                var curr = ordered[i];
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

            if (string.IsNullOrEmpty(group.PatientEmail))
                return BadRequest("Bệnh nhân không có email.");

            var subject = "Nhắc nhở điều trị - Bệnh viện Y học cổ truyền Nha Trang";
            var body = $@"
                <h2>Xin chào {group.PatientName},</h2>
                <p>Hệ thống ghi nhận bạn đã vắng mặt trong 2 ngày liên tiếp: Ngày {prevDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} và Ngày {lastDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}.</p>
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
    }
}
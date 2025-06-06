using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.BacSi.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using Project.Services.Features;

namespace Project.Areas.BacSi.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TreatmentSuspendedsController : ControllerBase
    {
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;
        private readonly IUserRepository _userRepository;
        private readonly JwtManager _jwtManager;
        private readonly EmailService _emailService;
        private readonly IAssignmentRepository _assignmentRepository;

        public TreatmentSuspendedsController(
            ITreatmentRecordRepository treatmentRecordRepository,
            IUserRepository userRepository,
            JwtManager jwtManager,
            EmailService emailService,
            IAssignmentRepository assignmentRepository)
        {
            _treatmentRecordRepository = treatmentRecordRepository;
            _userRepository = userRepository;
            _jwtManager = jwtManager;
            _emailService = emailService;
            _assignmentRepository = assignmentRepository;
        }

        [HttpPost("suspend")]
        public async Task<IActionResult> SuspendTreatment([FromBody] TreatmentSuspendedRequest request)
        {
            try
            {
                var token = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                    return Unauthorized("Chưa đăng nhập.");

                var (username, role) = _jwtManager.GetClaimsFromToken(token);
                if (string.IsNullOrEmpty(username))
                    return Unauthorized("Token không hợp lệ.");

                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null || user.Employee == null)
                    return NotFound("Không tìm thấy thông tin nhân viên.");

                var treatmentRecord = await _treatmentRecordRepository.GetByIdAdvancedAsync(request.TreatmentRecordId);
                if (treatmentRecord == null)
                    return NotFound("Không tìm thấy phiếu điều trị.");

                if (treatmentRecord.Status == TreatmentStatus.DaHuyBo || !string.IsNullOrEmpty(treatmentRecord.SuspendedBy))
                    return BadRequest("Phiếu điều trị đã bị đình chỉ hoặc đã hủy bỏ.");

                // Kiểm tra xem bác sĩ hiện tại có nằm trong danh sách phân công không
                var assignments = await _assignmentRepository.GetByTreatmentRecordIdAsync(treatmentRecord.Id);
                var isAssignedDoctor = assignments.Any(a => a.EmployeeId == user.Employee.Id);

                if (!isAssignedDoctor)
                    return Forbid("Bạn không có quyền đình chỉ phiếu điều trị này. Chỉ bác sĩ được phân công mới có quyền đình chỉ.");

                treatmentRecord.Status = request.Status;
                treatmentRecord.SuspendedReason = request.SuspendedReason;
                treatmentRecord.SuspendedNote = request.SuspendedNote;
                treatmentRecord.SuspendedBy = user.Employee.Code;
                treatmentRecord.SuspendedDate = DateTime.UtcNow;

                await _treatmentRecordRepository.UpdateAsync(treatmentRecord);

                // Gửi email thông báo đình chỉ phiếu điều trị cho bệnh nhân
                var patientEmail = treatmentRecord.Patient?.EmailAddress;
                if (!string.IsNullOrEmpty(patientEmail))
                {
                    var subject = "Thông báo đình chỉ đợt điều trị";
                    var body = $@"
                        <h2>Kính gửi {treatmentRecord.Patient?.Name},</h2>
                        <p>Chúng tôi xin thông báo phiếu điều trị <strong>{treatmentRecord.Code}</strong> (KCB từ ngày {treatmentRecord.StartDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} đến ngày {treatmentRecord.EndDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}) của bạn đã bị <strong>đình chỉ</strong>.</p>
                        <p>Lý do đình chỉ: <strong>{treatmentRecord.SuspendedReason}</strong></p>
                        <p>Vui lòng đến quầy tại bệnh viện để thanh toán chi phí sử dụng dịch vụ.</p>
                        <p>Nếu bạn có bất kỳ thắc mắc nào, vui lòng liên hệ với bệnh viện để được giải đáp.</p>
                        <p>Trân trọng,<br>Bệnh viện Y học cổ truyền Nha Trang</p>";

                    await _emailService.SendEmailAsync(patientEmail, subject, body);
                }
                else
                {
                    Console.WriteLine("DEBUG: Không có email bệnh nhân, không gửi mail");
                }

                return Ok(new { success = true, message = "Đình chỉ phiếu điều trị thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách các chuỗi 3 ngày liên tiếp vắng mặt (status = Không điều trị) của bệnh nhân trong 1 phiếu điều trị
        /// </summary>
        [HttpGet("absent-triples")]
        public async Task<IActionResult> GetAbsentTriples([FromQuery] Guid treatmentRecordId, [FromQuery] Guid roomId)
        {
            // Lấy phiếu điều trị
            var treatmentRecord = await _treatmentRecordRepository.GetByIdAdvancedAsync(treatmentRecordId);
            if (treatmentRecord == null)
                return NotFound("Không tìm thấy phiếu điều trị.");

            // Lấy các detail thuộc phòng này
            var detailsInRoom = treatmentRecord.TreatmentRecordDetails
                .Where(d => d.RoomId == roomId)
                .ToList();
            if (!detailsInRoom.Any())
                return Ok(new List<object>());

            // Lấy tất cả tracking của các detail này
            var allTrackings = new List<TreatmentTracking>();
            foreach (var detail in detailsInRoom)
            {
                if (detail.TreatmentTrackings != null)
                    allTrackings.AddRange(detail.TreatmentTrackings);
            }

            // Lọc các tracking có status = Không điều trị, sắp xếp theo ngày
            var absentTrackings = allTrackings
                .Where(t => t.Status == TrackingStatus.KhongDieuTri)
                .OrderBy(t => t.TrackingDate)
                .ToList();

            // Tìm các chuỗi 3 ngày liên tiếp
            var result = new List<object>();
            for (int i = 0; i <= absentTrackings.Count - 3; i++)
            {
                var t1 = absentTrackings[i];
                var t2 = absentTrackings[i + 1];
                var t3 = absentTrackings[i + 2];
                if ((t2.TrackingDate.Date - t1.TrackingDate.Date).TotalDays == 1 &&
                    (t3.TrackingDate.Date - t2.TrackingDate.Date).TotalDays == 1)
                {
                    result.Add(new
                    {
                        RoomId = roomId,
                        RoomName = t1.TreatmentRecordDetail!.Room?.Name,
                        Dates = new[] { t1.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), t2.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), t3.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) },
                        TrackingIds = new[] { t1.Id, t2.Id, t3.Id },
                        Notes = new[] { t1.Note, t2.Note, t3.Note }
                    });
                }
            }
            return Ok(result);
        }
    }
}

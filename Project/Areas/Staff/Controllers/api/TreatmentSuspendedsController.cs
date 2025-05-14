using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Staff.Models.DTOs;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using Project.Services.Features;

namespace Project.Areas.Staff.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TreatmentSuspendedsController : ControllerBase
    {
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;
        private readonly IUserRepository _userRepository;
        private readonly JwtManager _jwtManager;

        public TreatmentSuspendedsController(
            ITreatmentRecordRepository treatmentRecordRepository,
            IUserRepository userRepository,
            JwtManager jwtManager)
        {
            _treatmentRecordRepository = treatmentRecordRepository;
            _userRepository = userRepository;
            _jwtManager = jwtManager;
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

                var treatmentRecord = await _treatmentRecordRepository.GetByIdAsync(request.TreatmentRecordId);
                if (treatmentRecord == null)
                    return NotFound("Không tìm thấy phiếu điều trị.");

                if (treatmentRecord.Status == TreatmentStatus.DaKetThuc || !string.IsNullOrEmpty(treatmentRecord.SuspendedBy))
                    return BadRequest("Phiếu điều trị đã bị đình chỉ hoặc đã kết thúc.");

                treatmentRecord.Status = TreatmentStatus.DaKetThuc;
                treatmentRecord.SuspendedReason = request.SuspendedReason;
                treatmentRecord.SuspendedNote = request.SuspendedNote;
                treatmentRecord.SuspendedBy = user.Employee.Code;
                treatmentRecord.SuspendedDate = DateTime.UtcNow;

                await _treatmentRecordRepository.UpdateAsync(treatmentRecord);

                return Ok(new { success = true, message = "Đình chỉ phiếu điều trị thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server: " + ex.Message);
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
            var allTrackings = new List<Project.Areas.Staff.Models.Entities.TreatmentTracking>();
            foreach (var detail in detailsInRoom)
            {
                if (detail.TreatmentTrackings != null)
                    allTrackings.AddRange(detail.TreatmentTrackings);
            }

            // Lọc các tracking có status = Không điều trị, sắp xếp theo ngày
            var absentTrackings = allTrackings
                .Where(t => t.Status == Project.Models.Enums.TrackingStatus.KhongDieuTri && t.IsActive)
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
                    result.Add(new {
                        RoomId = roomId,
                        RoomName = t1.TreatmentRecordDetail!.Room?.Name,
                        Dates = new[] { t1.TrackingDate.ToString("yyyy-MM-dd"), t2.TrackingDate.ToString("yyyy-MM-dd"), t3.TrackingDate.ToString("yyyy-MM-dd") },
                        TrackingIds = new[] { t1.Id, t2.Id, t3.Id },
                        Notes = new[] { t1.Note, t2.Note, t3.Note }
                    });
                }
            }
            return Ok(result);
        }
    }
}

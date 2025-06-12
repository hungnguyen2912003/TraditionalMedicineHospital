using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.YTa.Models.DTOs;
using Project.Helpers;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using SequentialGuid;

namespace Project.Areas.YTa.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackingHandlesController : ControllerBase
    {
        private readonly ITreatmentTrackingRepository _trackingRepo;
        private readonly ITreatmentRecordDetailRepository _detailRepo;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserRepository _userRepository;
        private readonly JwtManager _jwtManager;
        private readonly CodeGeneratorHelper _codeGenerator;

        public TrackingHandlesController
        (
            ITreatmentTrackingRepository trackingRepo,
            ITreatmentRecordDetailRepository detailRepo,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository,
            JwtManager jwtManager,
            CodeGeneratorHelper codeGenerator
        )
        {
            _trackingRepo = trackingRepo;
            _detailRepo = detailRepo;
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _jwtManager = jwtManager;
            _codeGenerator = codeGenerator;
        }

        [HttpGet("patients-in-room")]
        public async Task<ActionResult<List<object>>> GetPatientsInRoom([FromQuery] string? date = null)
        {
            try
            {
                var token = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                    return Unauthorized("Chưa đăng nhập.");

                var (username, role) = _jwtManager.GetClaimsFromToken(token);
                if (string.IsNullOrEmpty(username))
                    return Unauthorized("Token không hợp lệ.");

                // Lấy thông tin nhân viên
                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null || user.Employee == null)
                    return NotFound("Không tìm thấy thông tin nhân viên.");

                // Lấy phòng của nhân viên
                var roomId = user.Employee.RoomId;
                if (roomId == Guid.Empty)
                    return NotFound("Không tìm thấy phòng cho bác sĩ này.");

                // Lấy ngày cần theo dõi
                DateTime targetDate = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);

                // Lấy tất cả treatmentRecordDetail trong phòng
                var details = await _detailRepo.GetDetailsByRoomIdAsync(roomId);

                // Lấy danh sách các bệnh nhân được theo dõi trong ngày
                var trackings = await _trackingRepo.GetAllAdvancedAsync();
                var trackedDetailIds = trackings
                    .Where(t => t.TrackingDate.Date == targetDate.Date && t.TreatmentRecordDetail != null && t.TreatmentRecordDetail.RoomId == roomId)
                    .Select(t => t.TreatmentRecordDetailId)
                    .ToHashSet();

                // Lọc ra các detail chưa được chấm trong ngày
                var detailsToTrack = details
                    .Where(d => !trackedDetailIds.Contains(d.Id))
                    .Select(d => new
                    {
                        detailId = d.Id,
                        patientId = d.TreatmentRecord.Patient.Id,
                        patientName = d.TreatmentRecord.Patient.Name,
                        roomName = d.Room?.Name,
                        treatmentMethod = d.Room?.TreatmentMethod?.Name
                    })
                    .ToList();

                return Ok(detailsToTrack);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTracking([FromBody] TreatmentTrackingCreateDto dto)
        {
            // Lấy token từ cookie
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return Unauthorized("Chưa đăng nhập.");

            var (username, role) = _jwtManager.GetClaimsFromToken(token);
            if (string.IsNullOrEmpty(username))
                return Unauthorized("Token không hợp lệ.");

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || user.Employee == null)
                return NotFound("Không tìm thấy thông tin nhân viên.");

            var employee = user.Employee;
            var code = await _codeGenerator.GenerateUniqueCodeAsync(_trackingRepo);
            var sequentialGuid = SequentialGuidGenerator.Instance.NewGuid();
            var now = DateTime.Now;

            // Tìm TreatmentRecordDetail theo PatientId và RoomId
            var detail = await _detailRepo.GetByPatientAndRoomAsync(dto.PatientId, dto.RoomId);
            if (detail == null)
                return NotFound("Không tìm thấy chi tiết điều trị cho bệnh nhân/phòng này.");

            // Tạo TreatmentTracking
            var tracking = new TreatmentTracking
            {
                Id = sequentialGuid,
                Code = code,
                TrackingDate = now,
                CreatedBy = employee.Code,
                CreatedDate = now,
                Status = dto.Status,
                Note = dto.Note,
                TreatmentRecordDetailId = detail.Id,
                EmployeeId = employee.Id
            };

            await _trackingRepo.CreateAsync(tracking);
            return Ok(new { success = true, message = "Lưu thành công!", id = tracking.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTracking(Guid id, [FromBody] UpdateTrackingDto dto)
        {
            try
            {
                // Lấy token từ cookie
                var token = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                    return Unauthorized("Chưa đăng nhập.");

                var (username, role) = _jwtManager.GetClaimsFromToken(token);
                if (string.IsNullOrEmpty(username))
                    return Unauthorized("Token không hợp lệ.");

                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null || user.Employee == null)
                    return NotFound("Không tìm thấy thông tin nhân viên.");

                // Kiểm tra xem tracking có tồn tại không
                var tracking = await _trackingRepo.GetByIdAsync(id);
                if (tracking == null)
                    return NotFound("Không tìm thấy theo dõi điều trị này.");

                // Kiểm tra quyền chỉnh sửa (chỉ người tạo mới được sửa)
                if (tracking.CreatedBy != user.Employee.Code)
                    return Forbid("Bạn không có quyền chỉnh sửa theo dõi điều trị này.");

                // Cập nhật thông tin
                tracking.Status = dto.Status;
                tracking.Note = dto.Note;
                tracking.UpdatedBy = user.Employee.Code;
                tracking.UpdatedDate = DateTime.Now;

                await _trackingRepo.UpdateAsync(tracking);
                return Ok(new { success = true, message = "Cập nhật thành công!", id = tracking.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("room-name")]
        public async Task<ActionResult<string>> GetRoomName()
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

            var employeeId = user.Employee.Id;
            var roomId = user.Employee.RoomId;
            var roomName = await _employeeRepository.GetRoomNameByEmployeeIdAsync(employeeId);
            if (roomName == null)
                return NotFound("Không tìm thấy phòng cho bác sĩ này.");

            return Ok(new { roomId, roomName });
        }
    }
}

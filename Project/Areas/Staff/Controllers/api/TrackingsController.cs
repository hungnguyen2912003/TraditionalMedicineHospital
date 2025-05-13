using Microsoft.AspNetCore.Mvc;
using Project.Areas.Staff.Models.DTOs.TrackingDTO;
using Project.Repositories.Interfaces;
using Project.Areas.Staff.Models.Entities;
using AutoMapper;
using Project.Services.Features;
using Project.Helpers;
using Hospital.Areas.Staff.Models.DTOs.TrackingDTO;
using Project.Models.Enums;
using SequentialGuid;

namespace Project.Areas.Staff.Controllers.api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrackingsController : ControllerBase
    {
        private readonly ITreatmentTrackingRepository _trackingRepo;
        private readonly ITreatmentRecordDetailRepository _detailRepo;
        private readonly IMapper _mapper;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserRepository _userRepository;
        private readonly JwtManager _jwtManager;
        private readonly CodeGeneratorHelper _codeGenerator;
        private readonly EmailService _emailService;

        public TrackingsController
        (
            ITreatmentTrackingRepository trackingRepo,
            ITreatmentRecordDetailRepository detailRepo,
            IMapper mapper,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository,
            JwtManager jwtManager,
            CodeGeneratorHelper codeGenerator,
            EmailService emailService
        )
        {
            _trackingRepo = trackingRepo;
            _detailRepo = detailRepo;
            _mapper = mapper;
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _jwtManager = jwtManager;
            _codeGenerator = codeGenerator;
            _emailService = emailService;
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

                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null || user.Employee == null)
                    return NotFound("Không tìm thấy thông tin nhân viên.");

                var roomId = user.Employee.RoomId;
                if (roomId == Guid.Empty)
                    return NotFound("Không tìm thấy phòng cho bác sĩ này.");

                DateTime targetDate = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);

                // Lấy tất cả bệnh nhân trong phòng
                var patients = await _detailRepo.GetPatientsByRoomIdAsync(roomId);

                // Lấy danh sách các tracking đã chấm trong ngày
                var trackings = await _trackingRepo.GetAllAdvancedAsync();
                var trackedPatientIds = trackings
                    .Where(t => t.TrackingDate.Date == targetDate.Date && t.TreatmentRecordDetail != null && t.TreatmentRecordDetail.RoomId == roomId)
                    .Select(t => t.TreatmentRecordDetail!.TreatmentRecord!.PatientId)
                    .ToHashSet();

                // Lọc ra các bệnh nhân chưa được chấm trong ngày
                var patientsToTrack = patients
                    .Where(p => !trackedPatientIds.Contains(p.Id))
                    .Select(p => new { id = p.Id, name = p.Name })
                    .ToList();

                return Ok(patientsToTrack);
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
                IsActive = true,
                Note = dto.Note,
                TreatmentRecordDetailId = detail.Id,
                EmployeeId = employee.Id
            };

            await _trackingRepo.CreateAsync(tracking);
            // Kiểm tra và gửi thông báo nếu có bệnh nhân vắng mặt
            if (tracking.Status == TrackingStatus.KhongDieuTri)
            {
                // Lấy tất cả các tracking cùng TreatmentRecordDetailId, trạng thái Không điều trị, còn hiệu lực
                var allTrackings = await _trackingRepo.GetAllAdvancedAsync();
                var relevantTrackings = allTrackings
                    .Where(t => t.TreatmentRecordDetailId == tracking.TreatmentRecordDetailId
                                && t.Status == TrackingStatus.KhongDieuTri
                                && t.IsActive)
                    .OrderBy(t => t.TrackingDate)
                    .ToList();

                // Để tránh gửi lặp lại trong 1 lần tạo
                var sentPairs = new HashSet<string>();

                for (int i = 0; i < relevantTrackings.Count - 1; i++)
                {
                    var first = relevantTrackings[i];
                    var second = relevantTrackings[i + 1];
                    var daysDiff = (second.TrackingDate.Date - first.TrackingDate.Date).TotalDays;
                    if (daysDiff == 1)
                    {
                        // Tạo key duy nhất cho cặp này
                        var pairKey = $"{first.Id}_{second.Id}";
                        if (!sentPairs.Contains(pairKey))
                        {
                            var patient = tracking.TreatmentRecordDetail?.TreatmentRecord?.Patient;
                            if (patient != null && !string.IsNullOrEmpty(patient.EmailAddress))
                            {
                                var subject = "Nhắc nhở điều trị - Bệnh viện Y học cổ truyền Nha Trang";
                                var body = $@"
                                    <h2>Xin chào {patient.Name},</h2>
                                    <p>Hệ thống ghi nhận bạn đã vắng mặt trong 2 ngày liên tiếp: Ngày {first.TrackingDate:dd/MM/yyyy} và Ngày {second.TrackingDate:dd/MM/yyyy}.</p>
                                    <p>Để đảm bảo hiệu quả điều trị, vui lòng sắp xếp thời gian đến bệnh viện để tiếp tục điều trị.</p>
                                    <p>Nếu bạn có lý do đặc biệt, vui lòng liên hệ với bác sĩ điều trị của bạn.</p>
                                    <p>Trân trọng,<br>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>";
                                await _emailService.SendEmailAsync(patient.EmailAddress, subject, body);
                                sentPairs.Add(pairKey);
                            }
                        }
                    }
                }
            }

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
                // Lấy lại bản ghi vừa update từ database
                var updatedTracking = await _trackingRepo.GetByIdAsync(tracking.Id);

                // Thực hiện kiểm tra và gửi mail trực tiếp
                if (updatedTracking!.Status == TrackingStatus.KhongDieuTri)
                {
                    var allTrackings = await _trackingRepo.GetAllAdvancedAsync();
                    var relevantTrackings = allTrackings
                        .Where(t => t.TreatmentRecordDetailId == updatedTracking.TreatmentRecordDetailId
                                    && t.Status == TrackingStatus.KhongDieuTri
                                    && t.IsActive)
                        .OrderBy(t => t.TrackingDate)
                        .ToList();

                    // Để tránh gửi lặp lại trong 1 lần cập nhật
                    var sentPairs = new HashSet<string>();

                    for (int i = 0; i < relevantTrackings.Count - 1; i++)
                    {
                        var first = relevantTrackings[i];
                        var second = relevantTrackings[i + 1];
                        var daysDiff = (second.TrackingDate.Date - first.TrackingDate.Date).TotalDays;
                        if (daysDiff == 1)
                        {
                            // Tạo key duy nhất cho cặp này
                            var pairKey = $"{first.Id}_{second.Id}";
                            if (!sentPairs.Contains(pairKey))
                            {
                                var patient = updatedTracking.TreatmentRecordDetail?.TreatmentRecord?.Patient;
                                if (patient != null && !string.IsNullOrEmpty(patient.EmailAddress))
                                {
                                    var subject = "Nhắc nhở điều trị - Bệnh viện Y học cổ truyền Nha Trang";
                                    var body = $@"
                                        <h2>Xin chào {patient.Name},</h2>
                                        <p>Hệ thống ghi nhận bạn đã vắng mặt trong 2 ngày liên tiếp: Ngày {first.TrackingDate:dd/MM/yyyy} và Ngày {second.TrackingDate:dd/MM/yyyy}.</p>
                                        <p>Để đảm bảo hiệu quả điều trị, vui lòng sắp xếp thời gian đến bệnh viện để tiếp tục điều trị.</p>
                                        <p>Nếu bạn có lý do đặc biệt, vui lòng liên hệ với bác sĩ điều trị của bạn.</p>
                                        <p>Trân trọng,<br>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>";
                                    await _emailService.SendEmailAsync(patient.EmailAddress, subject, body);
                                    sentPairs.Add(pairKey);
                                }
                            }
                        }
                    }
                }

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
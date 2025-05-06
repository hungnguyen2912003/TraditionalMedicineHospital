using Microsoft.AspNetCore.Mvc;
using Project.Areas.Staff.Models.DTOs.TrackingDTO;
using Project.Repositories.Interfaces;
using Project.Areas.Staff.Models.Entities;
using AutoMapper;
using Project.Services.Features;
using Project.Helpers;

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

        public TrackingsController
        (
            ITreatmentTrackingRepository trackingRepo,
            ITreatmentRecordDetailRepository detailRepo,
            IMapper mapper,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository,
            JwtManager jwtManager,
            CodeGeneratorHelper codeGenerator
        )
        {
            _trackingRepo = trackingRepo;
            _detailRepo = detailRepo;
            _mapper = mapper;
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _jwtManager = jwtManager;
            _codeGenerator = codeGenerator;
        }

        [HttpGet("patients-in-room")]
        public async Task<ActionResult<List<string>>> GetPatientsInRoom()
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

                var employeeId = user.Employee.Id;
                var roomId = user.Employee.RoomId;
                if (roomId == Guid.Empty)
                    return NotFound("Không tìm thấy phòng cho bác sĩ này.");

                var patients = await _detailRepo.GetPatientsByRoomIdAsync(roomId);
                if (patients == null)
                    return Ok(new List<object>());

                // Nếu muốn trả về cả id và name:
                return Ok(patients.Select(p => new { id = p.Id, name = p.Name }).ToList());
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
            var now = DateTime.Now;

            // Tìm TreatmentRecordDetail theo PatientId và RoomId
            var detail = await _detailRepo.GetByPatientAndRoomAsync(dto.PatientId, dto.RoomId);
            if (detail == null)
                return NotFound("Không tìm thấy chi tiết điều trị cho bệnh nhân/phòng này.");

            // Tạo TreatmentTracking
            var tracking = new TreatmentTracking
            {
                Id = Guid.NewGuid(),
                Code = code,
                TrackingDate = now,
                CreatedBy = employee.Code,
                CreatedDate = now,
                Status = dto.Status,
                IsActive = true,
                Note = dto.Note
            };

            await _trackingRepo.CreateAsync(tracking);

            detail.TreatmentTrackingId = tracking.Id;
            await _detailRepo.UpdateAsync(detail);

            return Ok(new { success = true, message = "Lưu thành công!" });
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
            var roomName = await _employeeRepository.GetRoomNameByEmployeeIdAsync(employeeId);
            if (roomName == null)
                return NotFound("Không tìm thấy phòng cho bác sĩ này.");

            return Ok(roomName);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.YTa.Models.DTOs;
using Project.Helpers;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using SequentialGuid;
using System.Globalization;

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
        private readonly EmailService _emailService;

        public TrackingHandlesController
        (
            ITreatmentTrackingRepository trackingRepo,
            ITreatmentRecordDetailRepository detailRepo,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository,
            JwtManager jwtManager,
            CodeGeneratorHelper codeGenerator,
            EmailService emailService
        )
        {
            _trackingRepo = trackingRepo;
            _detailRepo = detailRepo;
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

                // Lấy tất cả detail trong phòng
                var details = await _detailRepo.GetDetailsByRoomIdAsync(roomId);

                // Lấy danh sách các tracking đã chấm trong ngày (theo detailId)
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
            // Kiểm tra và gửi thông báo nếu có bệnh nhân vắng mặt
            if (tracking.Status == TrackingStatus.KhongDieuTri)
            {
                var allTrackings = await _trackingRepo.GetAllAdvancedAsync();
                var relevantTrackings = allTrackings
                    .Where(t => t.TreatmentRecordDetailId == tracking.TreatmentRecordDetailId
                                && t.Status == TrackingStatus.KhongDieuTri)
                    .OrderBy(t => t.TrackingDate)
                    .ToList();

                // Kiểm tra 3 ngày liên tiếp
                if (relevantTrackings.Count >= 3)
                {
                    var last = relevantTrackings[relevantTrackings.Count - 1];
                    var mid = relevantTrackings[relevantTrackings.Count - 2];
                    var first = relevantTrackings[relevantTrackings.Count - 3];
                    var daysDiff1 = (mid.TrackingDate.Date - first.TrackingDate.Date).TotalDays;
                    var daysDiff2 = (last.TrackingDate.Date - mid.TrackingDate.Date).TotalDays;
                    if (daysDiff1 == 1 && daysDiff2 == 1)
                    {
                        var patient = tracking.TreatmentRecordDetail?.TreatmentRecord?.Patient;
                        // Lấy đúng bác sĩ thuộc khoa/phòng của TreatmentRecordDetail
                        var roomDepartmentId = tracking.TreatmentRecordDetail?.Room?.DepartmentId;
                        var assignments = tracking.TreatmentRecordDetail?.TreatmentRecord?.Assignments;
                        Employee? doctor = null;
                        if (assignments != null && roomDepartmentId != null)
                        {
                            doctor = assignments
                                .Where(a => a.Employee != null && a.Employee.Room != null && a.Employee.Room.DepartmentId == roomDepartmentId)
                                .Select(a => a.Employee)
                                .FirstOrDefault();
                        }
                        string doctorName = doctor?.Name ?? "Không xác định";
                        if (patient != null && !string.IsNullOrEmpty(patient.EmailAddress))
                        {
                            var subject = "Đình chỉ phiếu điều trị - Bệnh viện Y học cổ truyền Nha Trang";
                            var body = $@"
                                <h2>Xin chào {patient.Name},</h2>
                                <p>Bạn đã tự ý bỏ điều trị quá 3 ngày liên tiếp: {first.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}, {mid.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}, {last.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}.</p>
                                <p>Bác sĩ chỉ định sẽ tiến hành đình chỉ phiếu điều trị của bạn.</p>
                                <p>Sau khi đình chỉ, bạn cần thực hiện thanh toán hóa đơn điều trị theo quy định của bệnh viện.</p>
                                <p>Trân trọng,<br>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>";
                            await _emailService.SendEmailAsync(patient.EmailAddress, subject, body);
                        }
                        return Ok(new { success = true, message = "Lưu thành công!", id = tracking.Id });
                    }
                }

                // Nếu không phải 3 ngày liên tiếp, kiểm tra 2 ngày liên tiếp
                if (relevantTrackings.Count >= 2)
                {
                    var last = relevantTrackings[relevantTrackings.Count - 1];
                    var prev = relevantTrackings[relevantTrackings.Count - 2];
                    var daysDiff = (last.TrackingDate.Date - prev.TrackingDate.Date).TotalDays;
                    if (daysDiff == 1)
                    {
                        var patient = tracking.TreatmentRecordDetail?.TreatmentRecord?.Patient;
                        if (patient != null && !string.IsNullOrEmpty(patient.EmailAddress))
                        {
                            var subject = "Nhắc nhở điều trị - Bệnh viện Y học cổ truyền Nha Trang";
                            var body = $@"
                                <h2>Xin chào {patient.Name},</h2>
                                <p>Hệ thống ghi nhận bạn đã vắng mặt trong 2 ngày liên tiếp: Ngày {prev.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} và Ngày {last.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}.</p>
                                <p>Để đảm bảo hiệu quả điều trị, vui lòng sắp xếp thời gian đến bệnh viện để tiếp tục điều trị.</p>
                    <p>Nếu bạn có lý do đặc biệt, vui lòng liên hệ với bác sĩ điều trị của bạn.</p>
                                <p>Trân trọng,<br>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>";
                            await _emailService.SendEmailAsync(patient.EmailAddress, subject, body);
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
                        .Where(t => t.TreatmentRecordDetailId == updatedTracking.TreatmentRecordDetailId)
                        .OrderBy(t => t.TrackingDate)
                        .ToList();

                    int idx = relevantTrackings.FindIndex(t => t.Id == updatedTracking.Id);
                    var patient = updatedTracking.TreatmentRecordDetail?.TreatmentRecord?.Patient;
                    if (patient != null && !string.IsNullOrEmpty(patient.EmailAddress))
                    {
                        // Cập nhật bản ghi đầu tiên
                        if (idx == 0 && relevantTrackings.Count > 1)
                        {
                            var next = relevantTrackings[1];
                            var daysDiff = (next.TrackingDate.Date - updatedTracking.TrackingDate.Date).TotalDays;
                            if (daysDiff == 1 &&
                                updatedTracking.Status == TrackingStatus.KhongDieuTri &&
                                next.Status == TrackingStatus.KhongDieuTri)
                            {
                                // Kiểm tra có phải 3 ngày liên tiếp không
                                if (relevantTrackings.Count > 2)
                                {
                                    var next2 = relevantTrackings[2];
                                    var daysDiff2 = (next2.TrackingDate.Date - next.TrackingDate.Date).TotalDays;
                                    if (daysDiff2 == 1 && next2.Status == TrackingStatus.KhongDieuTri)
                                    {
                                        // Lấy đúng bác sĩ thuộc khoa/phòng của TreatmentRecordDetail
                                        var doctorCode = tracking.TreatmentRecordDetail?.CreatedBy;
                                        string doctorName = "";
                                        if (!string.IsNullOrEmpty(doctorCode))
                                        {
                                            var doctor = await _employeeRepository.GetByCodeAsync(doctorCode);
                                            if (doctor != null)
                                                doctorName = doctor.Name ?? "Không xác định";
                                        }

                                        // Gửi mail đình chỉ
                                        var subject = "Đình chỉ phiếu điều trị - Bệnh viện Y học cổ truyền Nha Trang";
                                        var body = $@"
                                            <h2>Xin chào {patient.Name},</h2>
                                            <p>Bạn đã tự ý bỏ điều trị quá 3 ngày liên tiếp: {updatedTracking.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}, {next.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}, {next2.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}.</p>
                                            <p>Bác sĩ chỉ định sẽ tiến hành đình chỉ phiếu điều trị của bạn.</p>
                                            <p>Sau khi đình chỉ, bạn cần thực hiện thanh toán hóa đơn điều trị theo quy định của bệnh viện.</p>
                                            <p>Trân trọng,<br>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>";
                                        await _emailService.SendEmailAsync(patient.EmailAddress, subject, body);
                                    }
                                    else
                                    {
                                        // Gửi mail cho cặp updatedTracking-next
                                        var subject = "Nhắc nhở điều trị - Bệnh viện Y học cổ truyền Nha Trang";
                                        var body = $@"
                                            <h2>Xin chào {patient.Name},</h2>
                                            <p>Hệ thống ghi nhận bạn đã vắng mặt trong 2 ngày liên tiếp: Ngày {updatedTracking.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} và Ngày {next.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}.</p>
                                            <p>Để đảm bảo hiệu quả điều trị, vui lòng sắp xếp thời gian đến bệnh viện để tiếp tục điều trị.</p>
                                            <p>Nếu bạn có lý do đặc biệt, vui lòng liên hệ với bác sĩ điều trị của bạn.</p>
                                            <p>Trân trọng,<br>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>";
                                        await _emailService.SendEmailAsync(patient.EmailAddress, subject, body);
                                    }
                                }
                                else
                                {
                                    // Gửi mail cho cặp updatedTracking-next
                                    var subject = "Nhắc nhở điều trị - Bệnh viện Y học cổ truyền Nha Trang";
                                    var body = $@"
                                        <h2>Xin chào {patient.Name},</h2>
                                        <p>Hệ thống ghi nhận bạn đã vắng mặt trong 2 ngày liên tiếp: Ngày {updatedTracking.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} và Ngày {next.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}.</p>
                                        <p>Để đảm bảo hiệu quả điều trị, vui lòng sắp xếp thời gian đến bệnh viện để tiếp tục điều trị.</p>
                                        <p>Nếu bạn có lý do đặc biệt, vui lòng liên hệ với bác sĩ điều trị của bạn.</p>
                                        <p>Trân trọng,<br>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>";
                                    await _emailService.SendEmailAsync(patient.EmailAddress, subject, body);
                                }
                            }
                        }
                        // Cập nhật bản ghi cuối cùng
                        else if (idx == relevantTrackings.Count - 1 && relevantTrackings.Count > 1)
                        {
                            var prev = relevantTrackings[idx - 1];
                            var daysDiff = (updatedTracking.TrackingDate.Date - prev.TrackingDate.Date).TotalDays;
                            if (daysDiff == 1 &&
                                updatedTracking.Status == TrackingStatus.KhongDieuTri &&
                                prev.Status == TrackingStatus.KhongDieuTri)
                            {
                                if (relevantTrackings.Count > 2)
                                {
                                    var prev2 = relevantTrackings[idx - 2];
                                    var daysDiff2 = (prev.TrackingDate.Date - prev2.TrackingDate.Date).TotalDays;
                                    if (daysDiff2 == 1 && prev2.Status == TrackingStatus.KhongDieuTri)
                                    {
                                        // Lấy đúng bác sĩ thuộc khoa/phòng của TreatmentRecordDetail
                                        var doctorCode = tracking.TreatmentRecordDetail?.CreatedBy;
                                        string doctorName = "";
                                        if (!string.IsNullOrEmpty(doctorCode))
                                        {
                                            var doctor = await _employeeRepository.GetByCodeAsync(doctorCode);
                                            if (doctor != null)
                                                doctorName = doctor.Name;
                                        }

                                        // Gửi mail đình chỉ
                                        var subject = "Đình chỉ phiếu điều trị - Bệnh viện Y học cổ truyền Nha Trang";
                                        var body = $@"
                                            <h2>Xin chào {patient.Name},</h2>
                                            <p>Bạn đã tự ý bỏ điều trị quá 3 ngày liên tiếp: {prev2.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}, {prev.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}, {updatedTracking.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}.</p>
                                            <p>Bác sĩ chỉ định sẽ tiến hành đình chỉ phiếu điều trị của bạn.</p>
                                            <p>Sau khi đình chỉ, bạn cần thực hiện thanh toán hóa đơn điều trị theo quy định của bệnh viện.</p>
                                            <p>Trân trọng,<br>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>";
                                        await _emailService.SendEmailAsync(patient.EmailAddress, subject, body);
                                    }
                                    else
                                    {
                                        // Gửi mail cho cặp prev-updatedTracking
                                        var subject = "Nhắc nhở điều trị - Bệnh viện Y học cổ truyền Nha Trang";
                                        var body = $@"
                                            <h2>Xin chào {patient.Name},</h2>
                                            <p>Hệ thống ghi nhận bạn đã vắng mặt trong 2 ngày liên tiếp: Ngày {prev.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} và Ngày {updatedTracking.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}.</p>
                                            <p>Để đảm bảo hiệu quả điều trị, vui lòng sắp xếp thời gian đến bệnh viện để tiếp tục điều trị.</p>
                                            <p>Nếu bạn có lý do đặc biệt, vui lòng liên hệ với bác sĩ điều trị của bạn.</p>
                                            <p>Trân trọng,<br>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>";
                                        await _emailService.SendEmailAsync(patient.EmailAddress, subject, body);
                                    }
                                }
                                else
                                {
                                    // Gửi mail cho cặp prev-updatedTracking
                                    var subject = "Nhắc nhở điều trị - Bệnh viện Y học cổ truyền Nha Trang";
                                    var body = $@"
                                        <h2>Xin chào {patient.Name},</h2>
                                        <p>Hệ thống ghi nhận bạn đã vắng mặt trong 2 ngày liên tiếp: Ngày {prev.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} và Ngày {updatedTracking.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}.</p>
                                        <p>Để đảm bảo hiệu quả điều trị, vui lòng sắp xếp thời gian đến bệnh viện để tiếp tục điều trị.</p>
                                        <p>Nếu bạn có lý do đặc biệt, vui lòng liên hệ với bác sĩ điều trị của bạn.</p>
                                        <p>Trân trọng,<br>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>";
                                    await _emailService.SendEmailAsync(patient.EmailAddress, subject, body);
                                }
                            }
                        }
                        // Cập nhật bản ghi ở giữa
                        else if (idx > 0 && idx < relevantTrackings.Count - 1)
                        {
                            var prev = relevantTrackings[idx - 1];
                            var next = relevantTrackings[idx + 1];
                            bool prevPair = (updatedTracking.TrackingDate.Date - prev.TrackingDate.Date).TotalDays == 1 &&
                                            updatedTracking.Status == TrackingStatus.KhongDieuTri &&
                                            prev.Status == TrackingStatus.KhongDieuTri;
                            bool nextPair = (next.TrackingDate.Date - updatedTracking.TrackingDate.Date).TotalDays == 1 &&
                                            updatedTracking.Status == TrackingStatus.KhongDieuTri &&
                                            next.Status == TrackingStatus.KhongDieuTri;

                            // Nếu là 3 ngày liên tiếp thì chỉ gửi mail đình chỉ
                            if (prevPair && nextPair)
                            {
                                var subject = "Đình chỉ phiếu điều trị - Bệnh viện Y học cổ truyền Nha Trang";
                                var body = $@"
                                    <h2>Xin chào {patient.Name},</h2>
                                    <p>Bạn đã tự ý bỏ điều trị quá 3 ngày liên tiếp: {prev.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}, {updatedTracking.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}, {next.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}.</p>
                                    <p>Bác sĩ chỉ định sẽ tiến hành đình chỉ phiếu điều trị của bạn.</p>
                                    <p>Sau khi đình chỉ, bạn cần thực hiện thanh toán hóa đơn điều trị theo quy định của bệnh viện.</p>
                                    <p>Trân trọng,<br>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>";
                                await _emailService.SendEmailAsync(patient.EmailAddress, subject, body);
                            }
                            else
                            {
                                if (prevPair)
                                {
                                    var subject = "Nhắc nhở điều trị - Bệnh viện Y học cổ truyền Nha Trang";
                                    var body = $@"
                                        <h2>Xin chào {patient.Name},</h2>
                                        <p>Hệ thống ghi nhận bạn đã vắng mặt trong 2 ngày liên tiếp: Ngày {prev.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} và Ngày {updatedTracking.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}.</p>
                                        <p>Để đảm bảo hiệu quả điều trị, vui lòng sắp xếp thời gian đến bệnh viện để tiếp tục điều trị.</p>
                                        <p>Nếu bạn có lý do đặc biệt, vui lòng liên hệ với bác sĩ điều trị của bạn.</p>
                                        <p>Trân trọng,<br>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>";
                                    await _emailService.SendEmailAsync(patient.EmailAddress, subject, body);
                                }
                                if (nextPair)
                                {
                                    var subject = "Nhắc nhở điều trị - Bệnh viện Y học cổ truyền Nha Trang";
                                    var body = $@"
                                        <h2>Xin chào {patient.Name},</h2>
                                        <p>Hệ thống ghi nhận bạn đã vắng mặt trong 2 ngày liên tiếp: Ngày {updatedTracking.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} và Ngày {next.TrackingDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}.</p>
                                        <p>Để đảm bảo hiệu quả điều trị, vui lòng sắp xếp thời gian đến bệnh viện để tiếp tục điều trị.</p>
                                        <p>Nếu bạn có lý do đặc biệt, vui lòng liên hệ với bác sĩ điều trị của bạn.</p>
                                        <p>Trân trọng,<br>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>";
                                    await _emailService.SendEmailAsync(patient.EmailAddress, subject, body);
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

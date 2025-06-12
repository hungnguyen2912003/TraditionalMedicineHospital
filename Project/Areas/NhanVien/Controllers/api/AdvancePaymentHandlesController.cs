using System.Globalization;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.NhanVien.Models.DTOs;
using Project.Helpers;
using Project.Models.Enums;
using Project.Repositories.Implementations;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using SequentialGuid;

namespace Project.Areas.NhanVien.Controllers.api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdvancePaymentHandlesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly JwtManager _jwtManager;
        private readonly CodeGeneratorHelper _codeGenerator;
        private readonly IUserRepository _userRepository;
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;
        private readonly IAdvancePaymentRepository _advancePaymentRepository;
        private readonly EmailService _emailService;

        public AdvancePaymentHandlesController
        (
            IMapper mapper,
            JwtManager jwtManager,
            CodeGeneratorHelper codeGenerator,
            IUserRepository userRepository,
            ITreatmentRecordRepository treatmentRecordRepository,
            IAdvancePaymentRepository advancePaymentRepository,
            EmailService emailService
        )
        {
            _mapper = mapper;
            _jwtManager = jwtManager;
            _codeGenerator = codeGenerator;
            _userRepository = userRepository;
            _treatmentRecordRepository = treatmentRecordRepository;
            _advancePaymentRepository = advancePaymentRepository;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AdvancePaymentCreateRequest request)
        {
            try
            {
                // 1. Kiểm tra xác thực người dùng thông qua token
                var token = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                    return Unauthorized("Chưa đăng nhập.");

                // 2. Lấy thông tin username và role từ token
                var (username, role) = _jwtManager.GetClaimsFromToken(token);
                if (string.IsNullOrEmpty(username))
                    return Unauthorized("Token không hợp lệ.");

                // 3. Lấy thông tin người dùng từ database
                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null || user.Employee == null)
                    return NotFound("Không tìm thấy thông tin nhân viên.");

                // 4. Kiểm tra phiếu điều trị
                var treatmentRecord = await _treatmentRecordRepository.GetByIdAdvancedAsync(request.TreatmentRecordId);
                if (treatmentRecord == null)
                    return NotFound("Không tìm thấy phiếu điều trị.");

                // 5. Tạo mã tạm ứng mới
                var sequentialGuid = SequentialGuidGenerator.Instance.NewGuid();

                // 6. Tạo đối tượng AdvancePayment mới
                var advancePayment = new AdvancePayment
                {
                    Id = sequentialGuid,
                    Code = await _codeGenerator.GenerateUniqueCodeAsync(_advancePaymentRepository),
                    PaymentDate = request.PaymentDate,
                    Amount = request.Amount,
                    Note = request.Note,
                    TreatmentRecordId = request.TreatmentRecordId,
                    Status = PaymentStatus.ChuaThanhToan,
                    Type = null,
                    CreatedBy = user.Employee.Code,
                    CreatedDate = DateTime.UtcNow,
                    TreatmentRecord = treatmentRecord
                };

                // 7. Lưu thông tin tạm ứng vào database
                await _advancePaymentRepository.CreateAsync(advancePayment);

                // 8. Gửi email thông báo cho bệnh nhân nếu có email
                if (treatmentRecord.Patient?.EmailAddress != null)
                {
                    var emailSubject = $"Thông báo tạm ứng chi phí";
                    var emailBody = $@"
                        <h2>Thông báo tạm ứng</h2>
                        <p>Kính gửi {treatmentRecord.Patient.Name},</p>
                        <p>Chúng tôi xin thông báo về chi tiết tạm ứng của bạn về phiếu điều trị {treatmentRecord.Code}:</p>
                        <ul>
                            <li>Mã tạm ứng: {advancePayment.Code}</li>
                            <li>Ngày tạm ứng: {advancePayment.PaymentDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}</li>
                            <li>Số tiền tạm ứng: {advancePayment.Amount:N0} VNĐ</li>
                        </ul>
                        <p>Trân trọng,<br>Bệnh viện Y học cổ truyền</p>";

                    await _emailService.SendEmailAsync(treatmentRecord.Patient.EmailAddress, emailSubject, emailBody);
                }

                // 9. Trả về kết quả thành công
                return Ok(new { success = true, message = "Tạo phiếu tạm ứng thành công!", advancePaymentId = advancePayment.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateAdvancePaymentStatusRequest request)
        {
            try
            {
                // 1. Kiểm tra xác thực người dùng thông qua token
                var token = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                    return Unauthorized("Chưa đăng nhập.");

                // 2. Lấy thông tin username và role từ token
                var (username, role) = _jwtManager.GetClaimsFromToken(token);
                if (string.IsNullOrEmpty(username))
                    return Unauthorized("Token không hợp lệ.");

                // 3. Lấy thông tin người dùng từ database
                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null || user.Employee == null)
                    return NotFound("Không tìm thấy thông tin nhân viên.");

                // 4. Lấy thông tin phiếu tạm ứng
                var advancePayment = await _advancePaymentRepository.GetByIdAdvancedAsync(request.Id);
                if (advancePayment == null)
                    return NotFound("Không tìm thấy phiếu tạm ứng.");

                // 5. Cập nhật trạng thái
                advancePayment.Status = request.Status;
                advancePayment.UpdatedBy = user.Employee.Code;
                advancePayment.UpdatedDate = DateTime.UtcNow;

                // 6. Lưu thay đổi vào database
                await _advancePaymentRepository.UpdateAsync(advancePayment);

                // 7. Trả về kết quả thành công
                return Ok(new { success = true, message = "Cập nhật trạng thái tạm ứng thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }
    }
}

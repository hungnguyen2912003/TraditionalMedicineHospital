using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.NhanVien.Models.DTOs;
using Project.Helpers;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using SequentialGuid;
using System.Globalization;

namespace Project.Areas.NhanVien.Controllers.api
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentHandlesController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;
        private readonly JwtManager _jwtManager;
        private readonly CodeGeneratorHelper _codeGenerator;
        private readonly IUserRepository _userRepository;
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly ITreatmentRecordDetailRepository _treatmentRecordDetailRepository;
        private readonly ITreatmentTrackingRepository _treatmentTrackingRepository;
        private readonly EmailService _emailService;

        public PaymentHandlesController
        (
            IPaymentRepository paymentRepository,
            IMapper mapper,
            JwtManager jwtManager,
            CodeGeneratorHelper codeGenerator,
            IUserRepository userRepository,
            ITreatmentRecordRepository treatmentRecordRepository,
            IPrescriptionRepository prescriptionRepository,
            ITreatmentRecordDetailRepository treatmentRecordDetailRepository,
            ITreatmentTrackingRepository treatmentTrackingRepository,
            EmailService emailService
        )
        {
            _paymentRepository = paymentRepository;
            _mapper = mapper;
            _jwtManager = jwtManager;
            _codeGenerator = codeGenerator;
            _userRepository = userRepository;
            _treatmentRecordRepository = treatmentRecordRepository;
            _prescriptionRepository = prescriptionRepository;
            _treatmentRecordDetailRepository = treatmentRecordDetailRepository;
            _treatmentTrackingRepository = treatmentTrackingRepository;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PaymentCreateRequest request)
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

                // 4. Lấy thông tin phiếu điều trị để tính toán chi phí
                var tr = await _treatmentRecordRepository.GetByIdAdvancedAsync(request.TreatmentRecordId);
                if (tr == null) return NotFound("Không tìm thấy phiếu điều trị");

                // 5. Tính tổng chi phí đơn thuốc
                var prescriptions = tr.Prescriptions ?? new List<Prescription>();
                // Tính tổng chi phí = (giá thuốc * số lượng) cho từng chi tiết đơn thuốc
                var totalPrescriptionCost = prescriptions.Sum(p =>
                    p.PrescriptionDetails?.Sum(d => (d.Medicine?.Price ?? 0) * d.Quantity) ?? 0);

                // 6. Tính tổng chi phí phương pháp điều trị
                decimal totalTreatmentMethodCost = 0;
                // Tính tổng chi phí = (chi phí phương pháp * số lần điều trị) cho từng chi tiết điều trị
                foreach (var detail in tr.TreatmentRecordDetails ?? new List<TreatmentRecordDetail>())
                {
                    var room = detail.Room;
                    var method = room?.TreatmentMethod;
                    if (method == null) continue;
                    // Đếm số lần điều trị đã thực hiện (status = CoDieuTri)
                    int count = detail.TreatmentTrackings?.Count(t => t.Status == TrackingStatus.CoDieuTri) ?? 0;
                    totalTreatmentMethodCost += method.Cost * count;
                }

                // 7. Tính tổng chi phí trước khi áp dụng BHYT
                decimal totalCostBeforeInsurance = totalPrescriptionCost + totalTreatmentMethodCost;

                // 8. Tính số tiền được giảm từ BHYT
                decimal insuranceAmount = 0;
                var hi = tr.Patient?.HealthInsurance;
                if (hi != null && hi.ExpiryDate > DateTime.UtcNow)
                {
                    // Nếu đúng tuyến giảm 80%, trái tuyến giảm 60%
                    if (hi.IsRightRoute)
                        insuranceAmount = totalCostBeforeInsurance * 0.8m;
                    else
                        insuranceAmount = totalCostBeforeInsurance * 0.6m;
                }

                // 9. Tính số tiền cuối cùng bệnh nhân phải trả
                // (Tổng chi phí - BHYT - Tạm ứng), nếu âm thì = 0
                //decimal finalCost = totalCostBeforeInsurance - insuranceAmount - (tr.AdvancePayment ?? 0);
                decimal finalCost = totalCostBeforeInsurance - insuranceAmount;
                if (finalCost < 0) finalCost = 0;

                // 10. Tính số tiền thực tế bệnh nhân phải trả và số tiền tạm ứng còn dư
                decimal actualPatientPay = totalCostBeforeInsurance - insuranceAmount;
                //decimal advanceRefund = (tr.AdvancePayment ?? 0) - actualPatientPay;
                decimal advanceRefund = actualPatientPay;

                // 11. Tạo mã thanh toán mới
                var sequentialGuid = SequentialGuidGenerator.Instance.NewGuid();

                // 12. Tạo đối tượng Payment mới
                var payment = new Payment
                {
                    Id = sequentialGuid,
                    Code = await _codeGenerator.GenerateUniqueCodeAsync(_paymentRepository),
                    PaymentDate = request.PaymentDate,
                    Note = request.Note,
                    TreatmentRecordId = request.TreatmentRecordId,
                    // Nếu finalCost = 0 thì đã thanh toán xong, ngược lại chưa thanh toán
                    Status = finalCost == 0 ? PaymentStatus.DaThanhToan : PaymentStatus.ChuaThanhToan,
                    // Nếu finalCost = 0 thì thanh toán trực tiếp, ngược lại chưa xác định
                    Type = finalCost == 0 ? PaymentType.TrucTiep : null,
                    CreatedBy = user.Employee.Code,
                    CreatedDate = DateTime.UtcNow,
                    TreatmentRecord = null!
                };

                // 13. Lưu thông tin thanh toán vào database
                await _paymentRepository.CreateAsync(payment);

                // 14. Gửi email thông báo cho bệnh nhân nếu có email
                if (tr.Patient?.EmailAddress != null)
                {
                    var emailSubject = $"Thông báo thanh toán chi phí";
                    var emailBody = $@"
                        <h2>Thông báo thanh toán</h2>
                        <p>Kính gửi {tr.Patient.Name},</p>
                        <p>Chúng tôi xin thông báo về chi tiết thanh toán của bạn về phiếu điều trị {tr.Code}:</p>
                        <ul>
                            <li>Tổng tiền đơn thuốc: {totalPrescriptionCost:N0} VNĐ</li>
                            <li>Tổng tiền phương pháp điều trị: {totalTreatmentMethodCost:N0} VNĐ</li>
                            <li>Tiền giảm từ BHYT: {insuranceAmount:N0} VNĐ</li>
                            <li>Tạm ứng: {tr.AdvancePayment:N0} VNĐ</li>
                            <li><b>Tổng số tiền cần thanh toán trước khi chưa áp dụng tạm ứng: {totalCostBeforeInsurance - insuranceAmount:N0} VNĐ</b></li>
                            <li><b>Tổng số tiền cần thanh toán: {finalCost:N0} VNĐ</b></li>
                        </ul>";

                    // 15. Thêm thông tin về số tiền tạm ứng còn dư hoặc số tiền còn thiếu
                    if (advanceRefund > 0)
                    {
                        emailBody += $@"
                        <li>Số tiền tạm ứng còn dư: <strong>{advanceRefund:N0} VNĐ</strong></li>
                    </ul>
                    <p>Bạn cần đến bệnh viện để nhân viên tại quầy trả lại số tiền tạm ứng còn dư trên.</p>";
                    }
                    else if (advanceRefund < 0)
                    {
                        emailBody += $@"
                        <li>Số tiền còn thiếu: <strong>{Math.Abs(advanceRefund):N0} VNĐ</strong></li>
                    </ul>
                    <p>Bạn cần đến bệnh viện để thanh toán số tiền còn thiếu.</p>";
                    }
                    else
                    {
                        emailBody += $@"
                    </ul>";
                    }

                    emailBody += $@"
                <p>Trân trọng,<br>Bệnh viện Y học cổ truyền</p>";

                    // 16. Gửi email thông báo
                    await _emailService.SendEmailAsync(tr.Patient.EmailAddress, emailSubject, emailBody);
                }

                // 17. Trả về kết quả thành công
                return Ok(new { success = true, message = "Tạo phiếu thanh toán thành công!", paymentId = payment.Id });
            }
            catch (Exception ex)
            {
                // 18. Xử lý lỗi nếu có
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("CalculateCost/{treatmentRecordId}")]
        public async Task<IActionResult> CalculateCost(Guid treatmentRecordId)
        {
            try
            {
                // 1. Lấy thông tin TreatmentRecord
                var tr = await _treatmentRecordRepository.GetByIdAdvancedAsync(treatmentRecordId);
                if (tr == null) return NotFound("Không tìm thấy phiếu điều trị");

                // 2. Tổng tiền đơn thuốc
                var prescriptions = tr.Prescriptions ?? new List<Prescription>();
                var totalPrescriptionCost = prescriptions.Sum(p =>
                    p.PrescriptionDetails?.Sum(d => (d.Medicine?.Price ?? 0) * d.Quantity) ?? 0);

                // 3. Tổng tiền phương pháp điều trị
                decimal totalTreatmentMethodCost = 0;
                foreach (var detail in tr.TreatmentRecordDetails ?? new List<TreatmentRecordDetail>())
                {
                    var room = detail.Room;
                    var method = room?.TreatmentMethod;
                    if (method == null) continue;
                    // Đếm số lần điều trị (status = 1)
                    int count = detail.TreatmentTrackings?.Count(t => t.Status == TrackingStatus.CoDieuTri) ?? 0;
                    totalTreatmentMethodCost += method.Cost * count;
                }

                // 4. Tạm ứng
                //decimal advancePayment = (tr.AdvancePayment ?? 0);
                decimal advancePayment = 0;

                // 5. Tổng chi phí trước BHYT (KHÔNG trừ tạm ứng)
                decimal totalCostBeforeInsurance = totalPrescriptionCost + totalTreatmentMethodCost;

                // 6. Tính giảm BHYT
                decimal insuranceAmount = 0;
                var patient = tr.Patient;
                var hi = patient?.HealthInsurance;
                if (hi != null && hi.ExpiryDate > DateTime.UtcNow)
                {
                    if (hi.IsRightRoute)
                        insuranceAmount = totalCostBeforeInsurance * 0.8m;
                    else
                        insuranceAmount = totalCostBeforeInsurance * 0.6m;
                }
                // Nếu không có thẻ hoặc thẻ hết hạn thì insuranceAmount = 0

                // Số tiền thực tế bệnh nhân phải trả (không trừ tạm ứng, không ép về 0)
                decimal actualPatientPay = totalCostBeforeInsurance - insuranceAmount;

                // Số tiền cuối cùng bệnh nhân phải trả (đã trừ tạm ứng, ép về 0 nếu âm)
                decimal finalCost = actualPatientPay - advancePayment;
                if (finalCost < 0) finalCost = 0;

                return Ok(new
                {
                    totalPrescriptionCost,
                    totalTreatmentMethodCost,
                    insuranceAmount,
                    advancePayment,
                    totalCostBeforeInsurance,
                    actualPatientPay,
                    finalCost
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }

        [HttpPut("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdatePaymentStatusRequest request)
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
            var payment = await _paymentRepository.GetByIdAdvancedAsync(request.Id);
            if (payment == null)
                return NotFound("Không tìm thấy phiếu thanh toán");

            if (payment.Status != request.Status)
            {
                payment.Status = request.Status;
                payment.Type = PaymentType.TrucTiep;
                payment.UpdatedBy = user.Employee.Code;
                payment.UpdatedDate = DateTime.UtcNow;
            }
            await _paymentRepository.UpdateAsync(payment);
            return Ok(new { success = true, message = "Cập nhật trạng thái thành công!" });
        }

        [HttpGet("GetTreatmentDetails/{treatmentRecordId}")]
        public async Task<IActionResult> GetTreatmentDetails(Guid treatmentRecordId)
        {
            var tr = await _treatmentRecordRepository.GetByIdAdvancedAsync(treatmentRecordId);
            if (tr == null) return NotFound();
            var details = tr.TreatmentRecordDetails?.Select(detail => new
            {
                departmentName = detail.Room?.Department?.Name ?? "",
                roomName = detail.Room?.Name ?? "",
                methodName = detail.Room?.TreatmentMethod?.Name ?? "",
                cost = detail.Room?.TreatmentMethod?.Cost ?? 0,
                count = detail.TreatmentTrackings?.Count(t => t.Status == TrackingStatus.CoDieuTri) ?? 0
            }).ToList();
            if (details != null) return Ok(details);
            return Ok(new List<object>());
        }

        [HttpGet("GetPrescriptions/{treatmentRecordId}")]
        public async Task<IActionResult> GetPrescriptions(Guid treatmentRecordId)
        {
            var tr = await _treatmentRecordRepository.GetByIdAdvancedAsync(treatmentRecordId);
            if (tr == null) return NotFound();
            var prescriptions = tr.Prescriptions
                ?.OrderBy(p => p.PrescriptionDate)
                .Select(p => new
                {
                    code = p.Code,
                    createdDate = p.CreatedDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    totalCost = p.PrescriptionDetails?.Sum(d => (d.Medicine?.Price ?? 0) * d.Quantity) ?? 0
                }).ToList();
            if (prescriptions != null) return Ok(prescriptions);
            return Ok(new List<object>());
        }
    }
}
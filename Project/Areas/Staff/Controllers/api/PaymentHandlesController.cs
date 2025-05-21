using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Staff.Models.DTOs;
using Project.Areas.Staff.Models.Entities;
using Project.Helpers;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using Repositories.Interfaces;
using SequentialGuid;

namespace Project.Areas.Staff.Controllers.api
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
                var token = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                    return Unauthorized("Chưa đăng nhập.");

                var (username, role) = _jwtManager.GetClaimsFromToken(token);
                if (string.IsNullOrEmpty(username))
                    return Unauthorized("Token không hợp lệ.");

                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null || user.Employee == null)
                    return NotFound("Không tìm thấy thông tin nhân viên.");

                // Lấy thông tin TreatmentRecord để tính toán chi phí
                var tr = await _treatmentRecordRepository.GetByIdAdvancedAsync(request.TreatmentRecordId);
                if (tr == null) return NotFound("Không tìm thấy phiếu điều trị");

                // Tính toán các chi phí
                var prescriptions = tr.Prescriptions ?? new List<Prescription>();
                decimal totalPrescriptionCost = prescriptions.Sum(p => p.TotalCost);
                
                decimal totalTreatmentMethodCost = 0;
                foreach (var detail in tr.TreatmentRecordDetails ?? new List<TreatmentRecordDetail>())
                {
                    var room = detail.Room;
                    var method = room?.TreatmentMethod;
                    if (method == null) continue;
                    int count = detail.TreatmentTrackings?.Count(t => t.Status == TrackingStatus.CoDieuTri) ?? 0;
                    totalTreatmentMethodCost += method.Cost * count;
                }

                decimal totalCostBeforeInsurance = totalPrescriptionCost + totalTreatmentMethodCost;
                decimal insuranceAmount = 0;
                var hi = tr.Patient?.HealthInsurance;
                if (hi != null)
                {
                    if (hi.IsRightRoute)
                        insuranceAmount = totalCostBeforeInsurance * 0.8m;
                    else
                        insuranceAmount = totalCostBeforeInsurance * 0.6m;
                }

                decimal finalCost = totalCostBeforeInsurance - insuranceAmount - tr.AdvancePayment;
                if (finalCost < 0) finalCost = 0;

                // Tính số tiền tạm ứng còn dư hoặc số tiền còn thiếu
                decimal actualPatientPay = totalCostBeforeInsurance - insuranceAmount;
                decimal advanceRefund = tr.AdvancePayment - actualPatientPay;
                var sequentialGuid = SequentialGuidGenerator.Instance.NewGuid();

                var payment = new Payment
                {
                    Id = sequentialGuid,
                    Code = await _codeGenerator.GenerateUniqueCodeAsync(_paymentRepository),
                    PaymentDate = request.PaymentDate,
                    Note = request.Note,
                    TreatmentRecordId = request.TreatmentRecordId,
                    Status = PaymentStatus.ChuaThanhToan,
                    CreatedBy = user.Employee.Code,
                    CreatedDate = DateTime.UtcNow,
                    TreatmentRecord = null!
                };

                await _paymentRepository.CreateAsync(payment);

                // Gửi email cho bệnh nhân
                if (tr.Patient?.EmailAddress != null)
                {
                    var emailSubject = $"Thông báo thanh toán - Phiếu thanh toán {payment.Code}";
                    var emailBody = $@"
                        <h2>Thông báo thanh toán</h2>
                        <p>Kính gửi {tr.Patient.Name},</p>
                        <p>Chúng tôi xin thông báo về chi tiết thanh toán của bạn về phiếu điều trị {tr.Code}:</p>
                        <ul>
                            <li>Tổng tiền đơn thuốc: {totalPrescriptionCost:N0} VNĐ</li>
                            <li>Tổng tiền phương pháp điều trị: {totalTreatmentMethodCost:N0} VNĐ</li>
                            <li>Tiền giảm từ BHYT: {insuranceAmount:N0} VNĐ</li>
                            <li>Tạm ứng: {tr.AdvancePayment:N0} VNĐ</li>
                            <li><b>Tổng số tiền cần thanh toán trước khi chưa áp dụng tạm ứng: {(totalCostBeforeInsurance - insuranceAmount):N0} VNĐ</b></li>
                            <li><b>Tổng số tiền cần thanh toán: {finalCost:N0} VNĐ</b></li>
                        </ul>";

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

                await _emailService.SendEmailAsync(tr.Patient.EmailAddress, emailSubject, emailBody);
            }

            return Ok(new { success = true, message = "Tạo phiếu thanh toán thành công!", paymentId = payment.Id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

        [HttpGet("CalculateCost/{treatmentRecordId}")]
        public async Task<IActionResult> CalculateCost(Guid treatmentRecordId)
    {
        // 1. Lấy TreatmentRecord đầy đủ
        var tr = await _treatmentRecordRepository.GetByIdAdvancedAsync(treatmentRecordId);
        if (tr == null) return NotFound("Không tìm thấy phiếu điều trị");

        // 2. Tổng tiền đơn thuốc
        var prescriptions = tr.Prescriptions ?? new List<Project.Areas.Staff.Models.Entities.Prescription>();
        decimal totalPrescriptionCost = prescriptions.Sum(p => p.TotalCost);

        // 3. Tổng tiền phương pháp điều trị
        decimal totalTreatmentMethodCost = 0;
        foreach (var detail in tr.TreatmentRecordDetails ?? new List<Project.Areas.Staff.Models.Entities.TreatmentRecordDetail>())
        {
            var room = detail.Room;
            var method = room?.TreatmentMethod;
            if (method == null) continue;
            // Đếm số lần điều trị (status = 1)
            int count = detail.TreatmentTrackings?.Count(t => t.Status == TrackingStatus.CoDieuTri) ?? 0;
            totalTreatmentMethodCost += method.Cost * count;
        }

        // 4. Tạm ứng
        decimal advancePayment = tr.AdvancePayment;

        // 5. Tổng chi phí trước BHYT (KHÔNG trừ tạm ứng)
        decimal totalCostBeforeInsurance = totalPrescriptionCost + totalTreatmentMethodCost;

        // 6. Tính giảm BHYT
        decimal insuranceAmount = 0;
        var patient = tr.Patient;
        var hi = patient?.HealthInsurance;
        if (hi != null)
        {
            if (hi.IsRightRoute)
                insuranceAmount = totalCostBeforeInsurance * 0.8m;
            else
                insuranceAmount = totalCostBeforeInsurance * 0.6m;
        }
        // Nếu không có thẻ thì insuranceAmount = 0

        // Số tiền thực tế bệnh nhân phải trả (không trừ tạm ứng, không ép về 0)
        decimal actualPatientPay = totalCostBeforeInsurance - insuranceAmount;

        // 7. Tổng thanh toán cuối cùng
        decimal finalCost = totalCostBeforeInsurance - insuranceAmount - advancePayment;
        if (finalCost < 0) finalCost = 0;

        return Ok(new
        {
            totalPrescriptionCost,
            totalTreatmentMethodCost,
            insuranceAmount,
            advancePayment,
            finalCost,
            actualPatientPay
            });
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
                payment.UpdatedBy = user.Employee.Code;
                payment.UpdatedDate = DateTime.UtcNow;
            }
            await _paymentRepository.UpdateAsync(payment);
            return Ok(new { success = true, message = "Cập nhật trạng thái thành công!" });
        }
    }
}

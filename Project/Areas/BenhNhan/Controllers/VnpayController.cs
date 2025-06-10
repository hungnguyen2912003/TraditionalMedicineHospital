using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Library;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using Project.Services.Features;

namespace Project.Areas.BenhNhan.Controllers
{
    [Area("BenhNhan")]
    [Authorize(Roles = "BenhNhan")]
    [Route("vnpay")]
    public class VnpayController : Controller
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUserRepository _userRepository;
        private readonly VNPayService _vnPayService;
        private readonly JwtManager _jwtManager;
        private readonly IConfiguration _configuration;

        public VnpayController
        (
            IPaymentRepository paymentRepository,
            IUserRepository userRepository,
            VNPayService vnPayService,
            JwtManager jwtManager,
            IConfiguration configuration
        )
        {
            _paymentRepository = paymentRepository;
            _userRepository = userRepository;
            _vnPayService = vnPayService;
            _jwtManager = jwtManager;
            _configuration = configuration;
        }

        [HttpGet("return")]
        [AllowAnonymous]
        public async Task<IActionResult> VNPayReturn()
        {
            // Lấy các tham số trả về từ VNPay
            var vnp_ResponseCode = Request.Query["vnp_ResponseCode"].ToString();
            var vnp_TxnRef = Request.Query["vnp_TxnRef"].ToString();
            var vnp_Amount = Request.Query["vnp_Amount"].ToString();
            var vnp_OrderInfo = Request.Query["vnp_OrderInfo"].ToString();
            var vnp_SecureHash = Request.Query["vnp_SecureHash"].ToString();

            var parts = vnp_OrderInfo.Split('|');
            var paymentCode = parts[0];
            var userCode = parts.Length > 1 ? parts[1] : null;

            // Kiểm tra checksum
            var vnpay = new VnPayLibrary();
            foreach (var key in Request.Query.Keys)
            {
                if (key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, Request.Query[key]!);
                }
            }
            var hashSecret = _configuration["VNPaySettings:HashSecret"];
            bool isValidSignature = vnpay.ValidateSignature(vnp_SecureHash, hashSecret!);

            if (!isValidSignature)
            {
                ViewBag.Message = "Chữ ký không hợp lệ!";
                return View("VNPayReturn");
            }

            if (vnp_ResponseCode == "00")
            {
                // Tìm phiếu thanh toán theo mã giao dịch (orderId)
                var payment = await _paymentRepository.GetByCodeAsync(paymentCode);
                if (payment != null)
                {
                    payment.Status = PaymentStatus.DaThanhToan;
                    payment.Type = PaymentType.TrucTuyen;
                    payment.UpdatedDate = DateTime.UtcNow;
                    payment.UpdatedBy = userCode;
                    await _paymentRepository.UpdateAsync(payment);
                }
                ViewBag.PaymentCode = paymentCode;
                if (long.TryParse(vnp_Amount, out var amount))
                {
                    ViewBag.Amount = amount / 100;
                }
                else
                {
                    ViewBag.Amount = 0;
                }
            }
            else
            {
                ViewBag.Message = $"Thanh toán thất bại! Mã lỗi: {vnp_ResponseCode}";
            }
            return View();
        }
    }
}
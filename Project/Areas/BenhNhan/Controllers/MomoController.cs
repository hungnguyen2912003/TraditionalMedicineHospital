using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using Project.Services.Features;

namespace Project.Areas.BenhNhan.Controllers
{
    [Area("BenhNhan")]
    [Authorize(Roles = "BenhNhan")]
    [Route("momo")]
    public class MomoController : Controller
    {
        private readonly MomoService _momoService;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IAdvancePaymentRepository _advancePaymentRepository;
        private readonly IUserRepository _userRepository;
        private readonly JwtManager _jwtManager;
        private readonly IConfiguration _configuration;

        public MomoController(MomoService momoService, IPaymentRepository paymentRepository, IAdvancePaymentRepository advancePaymentRepository, IUserRepository userRepository, JwtManager jwtManager, IConfiguration configuration)
        {
            _momoService = momoService;
            _paymentRepository = paymentRepository;
            _advancePaymentRepository = advancePaymentRepository;
            _userRepository = userRepository;
            _jwtManager = jwtManager;
            _configuration = configuration;
        }

        [HttpGet("return")]
        [AllowAnonymous]
        public async Task<IActionResult> Return()
        {
            var orderId = Request.Query["orderId"].ToString();
            var orderInfo = Request.Query["orderInfo"].ToString();
            var amount = Request.Query["amount"].ToString();
            var errorCode = Request.Query["errorCode"].ToString();
            var message = Request.Query["message"].ToString();
            var extraData = Request.Query["extraData"].ToString();

            var payment = await _paymentRepository.GetByCodeAsync(orderInfo);
            var advancePayment = await _advancePaymentRepository.GetByCodeAsync(orderInfo);
            if (payment != null && errorCode == "0")
            {
                payment.Status = PaymentStatus.DaThanhToan;
                payment.Type = PaymentType.TrucTuyen;
                payment.UpdatedDate = DateTime.UtcNow;
                payment.UpdatedBy = extraData;
                await _paymentRepository.UpdateAsync(payment);

                ViewBag.PaymentCode = payment.Code;
                ViewBag.Amount = amount;
                ViewBag.Type = "phiếu thanh toán";
            }
            else if (advancePayment != null && errorCode == "0")
            {
                advancePayment.Status = PaymentStatus.DaThanhToan;
                advancePayment.Type = PaymentType.TrucTuyen;
                advancePayment.UpdatedDate = DateTime.UtcNow;
                advancePayment.UpdatedBy = extraData;
                await _advancePaymentRepository.UpdateAsync(advancePayment);

                ViewBag.AdvancePaymentCode = advancePayment.Code;
                ViewBag.Amount = amount;
                ViewBag.Type = "phiếu tạm ứng";
            }
            else
            {
                ViewBag.Message = "Thanh toán thất bại! Vui lòng báo cáo cho quản trị viên.";
            }
            return View();
        }

        [HttpPost("notify")]
        [AllowAnonymous]
        public async Task<IActionResult> Notify()
        {
            var orderId = Request.Form["orderId"].ToString();
            var resultCode = Request.Form["resultCode"].ToString();
            var message = Request.Form["message"].ToString();
            var amount = Request.Form["amount"].ToString();
            var extraData = Request.Form["extraData"].ToString();

            // Tìm phiếu thanh toán theo mã giao dịch (orderId)
            var payment = await _paymentRepository.GetByCodeAsync(orderId);
            var advancePayment = await _advancePaymentRepository.GetByCodeAsync(orderId);
            if (payment != null && resultCode == "0")
            {
                payment.Status = PaymentStatus.DaThanhToan;
                payment.Type = PaymentType.TrucTuyen;
                payment.UpdatedDate = DateTime.UtcNow;
                await _paymentRepository.UpdateAsync(payment);
            }
            else if (advancePayment != null && resultCode == "0")
            {
                advancePayment.Status = PaymentStatus.DaThanhToan;
                advancePayment.Type = PaymentType.TrucTuyen;
                advancePayment.UpdatedDate = DateTime.UtcNow;
                advancePayment.UpdatedBy = extraData;
                await _advancePaymentRepository.UpdateAsync(advancePayment);
            }
            // Trả về JSON cho MoMo biết đã nhận được thông báo
            return Json(new { message = "Received" });
        }
    }
}
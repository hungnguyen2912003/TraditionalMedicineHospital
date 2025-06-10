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
        private readonly IUserRepository _userRepository;
        private readonly JwtManager _jwtManager;
        private readonly IConfiguration _configuration;

        public MomoController(MomoService momoService, IPaymentRepository paymentRepository, IUserRepository userRepository, JwtManager jwtManager, IConfiguration configuration)
        {
            _momoService = momoService;
            _paymentRepository = paymentRepository;
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
            if (payment != null && errorCode == "0")
            {
                payment.Status = PaymentStatus.DaThanhToan;
                payment.Type = PaymentType.TrucTuyen;
                payment.UpdatedDate = DateTime.UtcNow;
                payment.UpdatedBy = extraData;
                await _paymentRepository.UpdateAsync(payment);

                ViewBag.PaymentCode = payment.Code;
                ViewBag.Amount = amount;
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

            // Tìm phiếu thanh toán theo mã giao dịch (orderId)
            var payment = await _paymentRepository.GetByCodeAsync(orderId);
            if (payment != null && resultCode == "0")
            {
                payment.Status = PaymentStatus.DaThanhToan;
                payment.Type = PaymentType.TrucTuyen;
                payment.UpdatedDate = DateTime.UtcNow;
                await _paymentRepository.UpdateAsync(payment);
            }
            // Trả về JSON cho MoMo biết đã nhận được thông báo
            return Json(new { message = "Received" });
        }
    }
}
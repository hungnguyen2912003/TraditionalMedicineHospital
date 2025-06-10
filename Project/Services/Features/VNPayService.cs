using Project.Library;

namespace Project.Services.Features
{
    public class VNPayService
    {
        private readonly string _tmnCode;
        private readonly string _hashSecret;
        private readonly string _baseUrl;
        private readonly string _returnUrl;

        public VNPayService(IConfiguration configuration)
        {
            _tmnCode = configuration["VNPaySettings:TmnCode"] ?? throw new ArgumentNullException("TmnCode configuration is missing");
            _hashSecret = configuration["VNPaySettings:HashSecret"] ?? throw new ArgumentNullException("HashSecret configuration is missing");
            _baseUrl = configuration["VNPaySettings:BaseUrl"] ?? throw new ArgumentNullException("BaseUrl configuration is missing");
            _returnUrl = configuration["VNPaySettings:ReturnUrl"] ?? throw new ArgumentNullException("ReturnUrl configuration is missing");
        }

        public string CreatePaymentUrl(decimal finalCost, string orderId, string orderInfo, string ipAddress)
        {
            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _tmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(finalCost * 100)).ToString());
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_TxnRef", orderId);
            vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _returnUrl);
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
            return vnpay.CreateRequestUrl(_baseUrl, _hashSecret);
        }
    }
}

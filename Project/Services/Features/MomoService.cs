using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Globalization;
using Newtonsoft.Json;
using Models;
using Project.Repositories.Interfaces;

namespace Project.Services.Features
{
    public class MomoService
    {
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _partnerCode;
        private readonly string _ApiUrl;
        private readonly string _returnUrl;
        private readonly string _notifyUrl;
        private readonly string _requestType;
        private readonly JwtManager _jwtManager;
        private readonly IUserRepository _userRepository;

        public MomoService(IConfiguration configuration, JwtManager jwtManager, IUserRepository userRepository)
        {
            _accessKey = configuration["MomoSettings:AccessKey"] ?? throw new ArgumentNullException("AccessKey configuration is missing");
            _secretKey = configuration["MomoSettings:SecretKey"] ?? throw new ArgumentNullException("SecretKey configuration is missing");
            _partnerCode = configuration["MomoSettings:PartnerCode"] ?? throw new ArgumentNullException("PartnerCode configuration is missing");
            _ApiUrl = configuration["MomoSettings:ApiUrl"] ?? throw new ArgumentNullException("ApiUrl configuration is missing");
            _returnUrl = configuration["MomoSettings:ReturnUrl"] ?? throw new ArgumentNullException("ReturnUrl configuration is missing");
            _notifyUrl = configuration["MomoSettings:NotifyUrl"] ?? throw new ArgumentNullException("NotifyUrl configuration is missing");
            _requestType = configuration["MomoSettings:RequestType"] ?? throw new ArgumentNullException("RequestType configuration is missing");
            _jwtManager = jwtManager;
            _userRepository = userRepository;
        }

        public async Task<string> CreatePaymentAsync(decimal amount, string orderId, string orderInfo, string userCode)
        {
            var requestId = orderId;
            var extraData = userCode;

            // Tạo raw signature string
            var rawHash = $"partnerCode={_partnerCode}&accessKey={_accessKey}&requestId={requestId}&amount={amount.ToString("0", CultureInfo.InvariantCulture)}&orderId={orderId}&orderInfo={orderInfo}&returnUrl={_returnUrl}&notifyUrl={_notifyUrl}&extraData={extraData}";
            var signature = ComputeHmacSha256(rawHash, _secretKey);

            var payload = new
            {
                partnerCode = _partnerCode,
                accessKey = _accessKey,
                requestId = requestId,
                amount = amount.ToString("0", CultureInfo.InvariantCulture),
                orderId = orderId,
                orderInfo = orderInfo,
                returnUrl = _returnUrl,
                notifyUrl = _notifyUrl,
                extraData = extraData,
                requestType = _requestType,
                signature = signature
            };

            using var client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_ApiUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }

        public MomoExecuteResponseModel PaymentExecute(IDictionary<string, string> collection)
        {
            // collection là các tham số trả về từ MoMo (query string)
            var amount = collection.ContainsKey("amount") ? collection["amount"] : "0";
            var orderInfo = collection.ContainsKey("orderInfo") ? collection["orderInfo"] : "";
            var orderId = collection.ContainsKey("orderId") ? collection["orderId"] : "";
            return new MomoExecuteResponseModel()
            {
                Amount = decimal.Parse(amount),
                OrderId = orderId,
                OrderInfo = orderInfo
            };
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
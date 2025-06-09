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
    }
}

using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Project.Services.Features
{
    public class SmsService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromNumber;

        public SmsService(IConfiguration configuration)
        {
            _accountSid = configuration["SmsSetting:accountSid"] ?? throw new ArgumentNullException("AccountSid configuration is missing");
            _authToken = configuration["SmsSetting:authToken"] ?? throw new ArgumentNullException("authToken configuration is missing");
            _fromNumber = configuration["SmsSetting:fromNumber"] ?? throw new ArgumentNullException("fromNumber configuration is missing");
        }

        public void SendSms(string to, string message)
        {
            var toWhatsapp = "whatsapp:" + to;
            TwilioClient.Init(_accountSid, _authToken);

            var messageResult = MessageResource.Create(
                to: new PhoneNumber(toWhatsapp),
                from: new PhoneNumber(_fromNumber),
                body: message
            );
        }
    }
}

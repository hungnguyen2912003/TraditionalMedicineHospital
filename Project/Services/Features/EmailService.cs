using System.Net.Mail;
using System.Net;

namespace Project.Services.Features
{
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            _smtpServer = configuration["EmailSettings:SmtpServer"] ?? throw new ArgumentNullException("SmtpServer configuration is missing");
            _smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"] ?? throw new ArgumentNullException("SmtpPort configuration is missing"));
            _smtpUsername = configuration["EmailSettings:SmtpUsername"] ?? throw new ArgumentNullException("SmtpUsername configuration is missing");
            _smtpPassword = configuration["EmailSettings:SmtpPassword"] ?? throw new ArgumentNullException("SmtpPassword configuration is missing");
            _fromEmail = configuration["EmailSettings:FromEmail"] ?? throw new ArgumentNullException("FromEmail configuration is missing");
            _fromName = configuration["EmailSettings:FromName"] ?? throw new ArgumentNullException("FromName configuration is missing");
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrEmpty(toEmail))
                throw new ArgumentNullException(nameof(toEmail));
            if (string.IsNullOrEmpty(subject))
                throw new ArgumentNullException(nameof(subject));
            if (string.IsNullOrEmpty(body))
                throw new ArgumentNullException(nameof(body));

            try
            {
                using var client = new SmtpClient(_smtpServer, _smtpPort)
                {
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
            catch (SmtpException ex)
            {
                throw new Exception($"Lỗi khi gửi email: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi không xác định khi gửi email: {ex.Message}", ex);
            }
        }
    }
}

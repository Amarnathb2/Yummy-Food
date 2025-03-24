using NETCore.MailKit.Core;

namespace YummyFood.Models
{
    public class EmailService
    {
        private readonly IEmailService _emailService;

        public EmailService(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string message)
        {
            await _emailService.SendAsync(recipientEmail, subject, message, true);
        }
    }
}

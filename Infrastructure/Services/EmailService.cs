using Application.Interface;
using Common.Configurations;
using Common.Models;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly EmailConfiguration _mailConfig;
        public EmailService(SmtpClient smtpClient,
                            IOptions<EmailConfiguration> options)
        {
            _smtpClient = smtpClient;
            _mailConfig = options.Value;
        }

        public async Task SendAsync(EmailModel email, CancellationToken cancellationToken)
        {
            email.From = String.IsNullOrEmpty(email.From) ? _mailConfig.Sender : email.From;
            MailAddress from = new MailAddress(email.From);
            MailAddress recipient = new MailAddress(email.To);

            var mailMessage = new MailMessage();
            mailMessage.To.Add(recipient);
            mailMessage.From = from;
            mailMessage.Subject = email.Subject;
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = email.Body;

            try
            {
                await _smtpClient.SendMailAsync(mailMessage, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email not sent. Status >>> {ex.Message}");
            }
        }
    }
}
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using Fringe.Domain.Models;

namespace Fringe.Service
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSetting> optionsAccessor, ILogger<EmailService> logger)
        {
            Options = optionsAccessor.Value;
            _logger = logger;
        }

        public EmailSetting Options { get; }

        public async Task SendEmailAsync(string emailTo, string subject, string htmlMessage)
        {
            if (string.IsNullOrEmpty(emailTo))
            {
                throw new ArgumentException("Recipient email is required.", nameof(emailTo));
            }

            using (var mailMessage = new MailMessage
            {
                From = new MailAddress(Options.SenderEmail, Options.SenderName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            })
            {
                mailMessage.To.Add(emailTo);

                using (var smtp = new SmtpClient
                {
                    Host = Options.Host,
                    Port = Options.Port,
                    EnableSsl = Options.EnableSsl,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(Options.Username, Options.Password),
                    DeliveryMethod = Options.DeliveryMethod == "Network"
                    ? SmtpDeliveryMethod.Network
                    : SmtpDeliveryMethod.SpecifiedPickupDirectory,
                    Timeout = 60000
                })
                {
                    await smtp.SendMailAsync(mailMessage);
                    _logger.LogInformation($"Email sent successfully to {emailTo}");
                }
            }
        }
    }
}

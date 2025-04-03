using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnterprisePMO_PWA.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Sends an email to the specified recipient
        /// </summary>
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_configuration["Email:SmtpServer"], 
                    int.Parse(_configuration["Email:SmtpPort"]))
                {
                    EnableSsl = true,
                    Credentials = new System.Net.NetworkCredential(
                        _configuration["Email:SmtpUsername"],
                        _configuration["Email:SmtpPassword"]
                    )
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(
                        _configuration["Email:FromEmail"],
                        _configuration["Email:FromName"]
                    ),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}");
                throw;
            }
        }
    }
} 
using Hangfire;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace EnterprisePMO_PWA.Application.Services
{
    /// <summary>
    /// Interface for sending notifications.
    /// </summary>
    public interface INotificationService
    {
        Task NotifyAsync(string message, string recipientEmail);
        void EnqueueEmailNotification(string recipientEmail, string subject, string body);
    }

    /// <summary>
    /// Implements INotificationService using Hangfire to process background email notifications.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly string _supabaseProjectRef;

        public NotificationService(IConfiguration configuration)
        {
            _supabaseProjectRef = configuration["Supabase:ProjectRef"] ?? throw new Exception("Supabase ProjectRef is not configured.");
        }

        /// <summary>
        /// Logs an in-app notification.
        /// </summary>
        public Task NotifyAsync(string message, string recipientEmail)
        {
            Console.WriteLine($"Notification to {recipientEmail}: {message}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Enqueues a background job to send an email.
        /// </summary>
        public void EnqueueEmailNotification(string recipientEmail, string subject, string body)
        {
            BackgroundJob.Enqueue(() => SendEmailAsync(recipientEmail, subject, body));
        }

        /// <summary>
        /// Sends an email via a Supabase serverless function.
        /// </summary>
        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            var supabaseEmailEndpoint = $"https://{_supabaseProjectRef}.supabase.co/functions/v1/send-email";
            using (var client = new HttpClient())
            {
                var payload = new
                {
                    email = recipientEmail,
                    subject = subject,
                    body = body
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(supabaseEmailEndpoint, content);
                response.EnsureSuccessStatusCode();
            }
        }
    }
}

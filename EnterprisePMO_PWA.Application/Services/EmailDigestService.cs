using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Supabase;

namespace EnterprisePMO_PWA.Application.Services
{
    public class EmailDigestService : BackgroundService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly EmailService _emailService;
        private readonly ILogger<EmailDigestService> _logger;

        public EmailDigestService(
            Supabase.Client supabaseClient,
            EmailService emailService,
            ILogger<EmailDigestService> logger)
        {
            _supabaseClient = supabaseClient;
            _emailService = emailService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessEmailDigests();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing email digests");
                }

                // Wait for 1 hour before next check
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task ProcessEmailDigests()
        {
            var now = DateTime.UtcNow;
            var preferences = await _supabaseClient
                .From<UserNotificationPreferences>()
                .Where(p => p.EmailDigestEnabled)
                .Get();

            foreach (var preference in preferences.Models)
            {
                if (ShouldSendDigest(preference, now))
                {
                    await SendDigestForUser(preference);
                }
            }
        }

        private bool ShouldSendDigest(UserNotificationPreferences preference, DateTime now)
        {
            var lastDigest = preference.LastDigestSent ?? DateTime.MinValue;
            var timeSinceLastDigest = now - lastDigest;

            return preference.EmailDigestFrequency switch
            {
                EmailDigestFrequency.Daily => timeSinceLastDigest.TotalHours >= 24,
                EmailDigestFrequency.Weekly => timeSinceLastDigest.TotalDays >= 7,
                EmailDigestFrequency.Biweekly => timeSinceLastDigest.TotalDays >= 14,
                EmailDigestFrequency.Monthly => timeSinceLastDigest.TotalDays >= 30,
                _ => false
            };
        }

        private async Task SendDigestForUser(UserNotificationPreferences preference)
        {
            var digestItems = await _supabaseClient
                .From<EmailDigestItem>()
                .Where(item => item.UserId == preference.UserId.ToString() && !item.Processed)
                .Get();

            if (!digestItems.Models.Any())
            {
                return;
            }

            var notifications = digestItems.Models
                .Select(item => item.Notification)
                .Where(n => n != null)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            var subject = $"Your {preference.EmailDigestFrequency} PMO Update Digest";
            var body = GenerateDigestEmailBody(notifications);

            var user = await _supabaseClient
                .From<User>()
                .Where(u => u.Id.ToString() == preference.UserId.ToString())
                .Single();

            if (user != null)
            {
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
            else
            {
                _logger.LogError($"User not found for ID: {preference.UserId}");
                return;
            }

            // Mark items as processed
            foreach (var item in digestItems.Models)
            {
                item.Processed = true;
                await _supabaseClient
                    .From<EmailDigestItem>()
                    .Update(item);
            }

            // Update last digest sent time
            preference.LastDigestSent = DateTime.UtcNow;
            await _supabaseClient
                .From<UserNotificationPreferences>()
                .Update(preference);
        }

        private string GenerateDigestEmailBody(List<Notification> notifications)
        {
            var body = new System.Text.StringBuilder();
            body.AppendLine("<h1>Your PMO Update Digest</h1>");
            body.AppendLine("<p>Here are your recent updates:</p>");
            body.AppendLine("<ul>");

            foreach (var notification in notifications)
            {
                body.AppendLine($"<li><strong>{notification.Title}</strong> - {notification.Message}</li>");
            }

            body.AppendLine("</ul>");
            body.AppendLine("<p>You can view these updates in detail by logging into the PMO system.</p>");

            return body.ToString();
        }
    }
} 
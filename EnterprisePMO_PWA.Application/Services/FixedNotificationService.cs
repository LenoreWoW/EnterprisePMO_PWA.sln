using Hangfire;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Services;
using EnterprisePMO_PWA.Infrastructure.Data;

namespace EnterprisePMO_PWA.Application.Services
{
    /// <summary>
    /// Enhanced notification service that supports both in-app and email notifications,
    /// with database storage and optional real-time notifications.
    /// </summary>
    public class EnhancedNotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IRealTimeNotificationService? _realTimeNotificationService;
        private readonly string _supabaseProjectRef;

        public EnhancedNotificationService(
            AppDbContext context,
            IConfiguration configuration,
            IRealTimeNotificationService? realTimeNotificationService = null)
        {
            _context = context;
            _configuration = configuration;
            _realTimeNotificationService = realTimeNotificationService;
            _supabaseProjectRef = configuration["Supabase:ProjectRef"] ?? throw new Exception("Supabase ProjectRef is not configured.");
        }

        /// <summary>
        /// Creates and sends an in-app notification to a user.
        /// </summary>
        public async Task NotifyAsync(string message, string recipientEmail)
        {
            // Find the recipient user
            var recipient = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == recipientEmail);

            if (recipient == null)
            {
                // Log that we couldn't find the user
                Console.WriteLine($"Warning: Notification recipient not found: {recipientEmail}");
                return;
            }

            // Create notification entity
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = recipient.Id,
                Message = message,
                Type = NotificationType.SystemAlert, // Default type
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            // Store in database
            _context.Add(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification if the service is available
            if (_realTimeNotificationService != null)
            {
                await _realTimeNotificationService.SendToUserAsync(recipient.Id, message);
            }

            // Log for debugging
            Console.WriteLine($"Notification to {recipientEmail}: {message}");
        }

        /// <summary>
        /// Sends a notification with more detailed metadata.
        /// </summary>
        public async Task NotifyWithMetadataAsync(
            string message, 
            string recipientEmail, 
            NotificationType type, 
            string? link = null, 
            Guid? entityId = null, 
            string? entityType = null)
        {
            // Find the recipient user
            var recipient = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == recipientEmail);

            if (recipient == null)
            {
                // Log that we couldn't find the user
                Console.WriteLine($"Warning: Notification recipient not found: {recipientEmail}");
                return;
            }

            // Create notification entity
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = recipient.Id,
                Message = message,
                Type = type,
                Link = link,
                EntityId = entityId,
                EntityType = entityType,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            // Store in database
            _context.Add(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification if the service is available
            if (_realTimeNotificationService != null)
            {
                await _realTimeNotificationService.SendToUserAsync(recipient.Id, message);
            }
        }

        /// <summary>
        /// Sends a notification to multiple recipients.
        /// </summary>
        public async Task NotifyMultipleAsync(string message, IEnumerable<string> recipientEmails, NotificationType type)
        {
            foreach (var email in recipientEmails)
            {
                await NotifyAsync(message, email);
            }
        }

        /// <summary>
        /// Enqueues an email notification to be sent asynchronously.
        /// </summary>
        public void EnqueueEmailNotification(string recipientEmail, string subject, string body)
        {
            // Simple implementation that doesn't use expression trees
            Task.Run(async () => {
                try {
                    await SendEmailWithoutOptionals(recipientEmail, subject, body);
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error in background email task: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Sends an email without optional parameters (used by Hangfire).
        /// </summary>
        public async Task SendEmailWithoutOptionals(string recipientEmail, string subject, string body)
        {
            await SendEmailAsync(recipientEmail, subject, body);
        }

        /// <summary>
        /// Sends an email via Supabase serverless function.
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
                
                try
                {
                    var response = await client.PostAsync(supabaseEmailEndpoint, content);
                    response.EnsureSuccessStatusCode();
                    
                    // Mark any related notifications as having emails sent
                    await UpdateNotificationEmailStatusAsync(recipientEmail, subject);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email to {recipientEmail}: {ex.Message}");
                    // Don't rethrow - email sending is "best effort"
                }
            }
        }

        /// <summary>
        /// Gets unread notifications for a user.
        /// </summary>
        public async Task<List<Notification>> GetUnreadNotificationsAsync(Guid userId)
        {
            return await _context.Set<Notification>()
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all notifications for a user with pagination.
        /// </summary>
        public async Task<(List<Notification> Notifications, int TotalCount)> GetNotificationsAsync(
            Guid userId, int page = 1, int pageSize = 20)
        {
            var query = _context.Set<Notification>()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync();

            var notifications = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (notifications, totalCount);
        }

        /// <summary>
        /// Gets the count of unread notifications for a user.
        /// </summary>
        public async Task<int> GetUnreadNotificationCountAsync(Guid userId)
        {
            return await _context.Set<Notification>()
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        /// <summary>
        /// Marks a notification as read.
        /// </summary>
        public async Task MarkNotificationAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Set<Notification>()
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Marks all notifications for a user as read.
        /// </summary>
        public async Task MarkAllNotificationsAsReadAsync(Guid userId)
        {
            var unreadNotifications = await _context.Set<Notification>()
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unreadNotifications.Any())
            {
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Deletes a notification.
        /// </summary>
        public async Task DeleteNotificationAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Set<Notification>()
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null)
            {
                _context.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Updates the email sent status for notifications.
        /// </summary>
        private async Task UpdateNotificationEmailStatusAsync(string recipientEmail, string subject)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == recipientEmail);

                if (user != null)
                {
                    // Find notifications that may match this email subject
                    // This is a best-effort match based on the subject line
                    var notifications = await _context.Set<Notification>()
                        .Where(n => n.UserId == user.Id && !n.EmailSent && n.Message.Contains(subject.Split(':').LastOrDefault() ?? ""))
                        .ToListAsync();

                    foreach (var notification in notifications)
                    {
                        notification.EmailSent = true;
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Just log, don't let this secondary operation fail the email send
                Console.WriteLine($"Error updating notification email status: {ex.Message}");
            }
        }
    }
}
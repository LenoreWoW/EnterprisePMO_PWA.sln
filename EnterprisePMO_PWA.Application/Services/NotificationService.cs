using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Enums;
using Microsoft.Extensions.Logging;
using Supabase;
using System.Text.Json;
using Supabase.Postgrest;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using static Supabase.Postgrest.Constants;
using EnterprisePMO_PWA.Domain.Interfaces;

namespace EnterprisePMO_PWA.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly ILogger<NotificationService> _logger;
        private readonly EmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPushNotificationService _pushNotificationService;

        public NotificationService(
            Supabase.Client supabaseClient,
            ILogger<NotificationService> logger,
            EmailService emailService,
            IUnitOfWork unitOfWork,
            IPushNotificationService pushNotificationService)
        {
            _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _unitOfWork = unitOfWork;
            _pushNotificationService = pushNotificationService;
        }

        /// <summary>
        /// Creates and sends an in-app notification to a user.
        /// </summary>
        public async Task NotifyAsync(string message, string recipientEmail)
        {
            try
            {
                // Get user by email
                var response = await _supabaseClient
                    .From<User>()
                    .Filter("username", Operator.Equals, recipientEmail)
                    .Single();

                if (response == null)
                {
                    _logger.LogWarning($"User with email {recipientEmail} not found");
                    return;
                }

                var userId = response.Id.ToString();

                // Create notification
                var notification = new Notification
                {
                    UserId = userId,
                    Title = "System Notification",
                    Message = message,
                    Type = NotificationType.SystemAlert,
                    Priority = NotificationPriority.Medium,
                    Read = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _supabaseClient
                    .From<Notification>()
                    .Insert(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to {RecipientEmail}", recipientEmail);
            }
        }

        /// <summary>
        /// Sends a notification with more detailed metadata.
        /// </summary>
        public async Task NotifyWithMetadataAsync(string message, string recipientEmail, NotificationType type, string? link = null, Guid? entityId = null, string? entityType = null)
        {
            try
            {
                // Get user by email
                var response = await _supabaseClient
                    .From<User>()
                    .Filter("username", Operator.Equals, recipientEmail)
                    .Single();

                if (response == null)
                {
                    _logger.LogWarning($"User with email {recipientEmail} not found");
                    return;
                }

                var userId = response.Id.ToString();

                // Create notification
                var notification = new Notification
                {
                    UserId = userId,
                    Title = "System Notification",
                    Message = message,
                    Type = type,
                    ActionUrl = link,
                    RelatedEntityId = entityId?.ToString(),
                    RelatedEntityType = entityType,
                    Priority = NotificationPriority.Medium,
                    Read = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _supabaseClient
                    .From<Notification>()
                    .Insert(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification with metadata to {RecipientEmail}", recipientEmail);
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
            // This is a placeholder implementation
            // In a real application, you would use a background job system like Hangfire
            Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendEmailAsync(recipientEmail, subject, body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending email to {RecipientEmail}", recipientEmail);
                }
            });
        }

        /// <summary>
        /// Gets unread notifications for a user.
        /// </summary>
        public async Task<List<Notification>> GetUnreadNotificationsAsync(Guid userId)
        {
            try
            {
                var response = await _supabaseClient
                    .From<Notification>()
                    .Filter("user_id", Operator.Equals, userId.ToString())
                    .Filter("read", Operator.Equals, false)
                    .Order("created_at", Ordering.Descending)
                    .Get();

                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notifications for user {UserId}", userId);
                return new List<Notification>();
            }
        }

        /// <summary>
        /// Gets all notifications for a user with pagination.
        /// </summary>
        public async Task<(List<Notification> Notifications, int TotalCount)> GetNotificationsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var offset = (page - 1) * pageSize;
                
                var response = await _supabaseClient
                    .From<Notification>()
                    .Filter("user_id", Operator.Equals, userId.ToString())
                    .Order("created_at", Ordering.Descending)
                    .Range(offset, offset + pageSize - 1)
                    .Get();

                var countResponse = await _supabaseClient
                    .From<Notification>()
                    .Filter("user_id", Operator.Equals, userId.ToString())
                    .Count(CountType.Exact);

                return (response.Models.ToList(), countResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                return (new List<Notification>(), 0);
            }
        }

        /// <summary>
        /// Gets the count of unread notifications for a user.
        /// </summary>
        public async Task<int> GetUnreadNotificationCountAsync(Guid userId)
        {
            try
            {
                var response = await _supabaseClient
                    .From<Notification>()
                    .Filter("user_id", Operator.Equals, userId.ToString())
                    .Filter("read", Operator.Equals, false)
                    .Count(CountType.Exact);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count for user {UserId}", userId);
                return 0;
            }
        }

        /// <summary>
        /// Marks a notification as read.
        /// </summary>
        public async Task MarkNotificationAsReadAsync(Guid notificationId, Guid userId)
        {
            try
            {
                var notification = new Notification
                {
                    Id = notificationId,
                    UserId = userId.ToString(),
                    Read = true,
                    ReadAt = DateTime.UtcNow
                };

                await _supabaseClient
                    .From<Notification>()
                    .Update(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}", notificationId, userId);
            }
        }

        /// <summary>
        /// Marks all notifications for a user as read.
        /// </summary>
        public async Task MarkAllNotificationsAsReadAsync(Guid userId)
        {
            try
            {
                var notification = new Notification
                {
                    Read = true,
                    ReadAt = DateTime.UtcNow
                };

                await _supabaseClient
                    .From<Notification>()
                    .Filter("user_id", Operator.Equals, userId.ToString())
                    .Filter("read", Operator.Equals, false)
                    .Update(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Deletes a notification.
        /// </summary>
        public async Task DeleteNotificationAsync(Guid notificationId, Guid userId)
        {
            try
            {
                await _supabaseClient
                    .From<Notification>()
                    .Filter("id", Operator.Equals, notificationId.ToString())
                    .Filter("user_id", Operator.Equals, userId.ToString())
                    .Delete();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId} for user {UserId}", notificationId, userId);
            }
        }

        public async Task<UserNotificationPreferences> GetUserNotificationPreferencesAsync(Guid userId)
        {
            var preferences = await _unitOfWork.UserNotificationPreferences.GetByUserIdAsync(userId);
            return preferences ?? new UserNotificationPreferences { UserId = userId };
        }

        public async Task UpdateUserNotificationPreferencesAsync(Guid userId, UserNotificationPreferences preferences)
        {
            var existingPreferences = await _unitOfWork.UserNotificationPreferences.GetByUserIdAsync(userId);
            if (existingPreferences == null)
            {
                preferences.UserId = userId;
                await _unitOfWork.UserNotificationPreferences.AddAsync(preferences);
            }
            else
            {
                existingPreferences.EmailEnabled = preferences.EmailEnabled;
                existingPreferences.PushEnabled = preferences.PushEnabled;
                existingPreferences.ProjectUpdates = preferences.ProjectUpdates;
                existingPreferences.TaskUpdates = preferences.TaskUpdates;
                existingPreferences.CommentUpdates = preferences.CommentUpdates;
                await _unitOfWork.UserNotificationPreferences.UpdateAsync(existingPreferences);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId)
        {
            var notification = await _unitOfWork.Notifications.GetByUserIdAsync(userId);
            return notification != null ? new[] { notification } : Enumerable.Empty<Notification>();
        }

        public async Task SubscribeToProjectAsync(Guid userId, Guid projectId)
        {
            var subscription = new ProjectSubscription
            {
                UserId = userId,
                ProjectId = projectId
            };
            await _unitOfWork.ProjectSubscriptions.AddAsync(subscription);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UnsubscribeFromProjectAsync(Guid userId, Guid projectId)
        {
            var subscription = await _unitOfWork.ProjectSubscriptions.GetByUserIdAndProjectIdAsync(userId, projectId);
            if (subscription != null)
            {
                await _unitOfWork.ProjectSubscriptions.DeleteAsync(subscription);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
} 
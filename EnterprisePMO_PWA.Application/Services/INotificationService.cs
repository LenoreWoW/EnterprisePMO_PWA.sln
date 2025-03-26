using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;

namespace EnterprisePMO_PWA.Application.Services
{
    /// <summary>
    /// Defines methods for sending and managing notifications.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Creates and sends an in-app notification to a user.
        /// </summary>
        Task NotifyAsync(string message, string recipientEmail);

        /// <summary>
        /// Enqueues an email notification to be sent asynchronously.
        /// </summary>
        void EnqueueEmailNotification(string recipientEmail, string subject, string body);

        /// <summary>
        /// Gets unread notifications for a user.
        /// </summary>
        Task<List<Notification>> GetUnreadNotificationsAsync(Guid userId);

        /// <summary>
        /// Gets all notifications for a user with pagination.
        /// </summary>
        Task<(List<Notification> Notifications, int TotalCount)> GetNotificationsAsync(
            Guid userId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Gets the count of unread notifications for a user.
        /// </summary>
        Task<int> GetUnreadNotificationCountAsync(Guid userId);

        /// <summary>
        /// Marks a notification as read.
        /// </summary>
        Task MarkNotificationAsReadAsync(Guid notificationId, Guid userId);

        /// <summary>
        /// Marks all notifications for a user as read.
        /// </summary>
        Task MarkAllNotificationsAsReadAsync(Guid userId);

        /// <summary>
        /// Deletes a notification.
        /// </summary>
        Task DeleteNotificationAsync(Guid notificationId, Guid userId);
    }
}
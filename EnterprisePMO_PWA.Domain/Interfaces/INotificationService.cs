using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Enums;

namespace EnterprisePMO_PWA.Domain.Interfaces
{
    public interface INotificationService
    {
        Task NotifyAsync(string message, string recipientEmail);
        Task NotifyWithMetadataAsync(string message, string recipientEmail, NotificationType type, string? link = null, Guid? entityId = null, string? entityType = null);
        Task NotifyMultipleAsync(string message, IEnumerable<string> recipientEmails, NotificationType type);
        Task<List<Notification>> GetUnreadNotificationsAsync(Guid userId);
        Task<(List<Notification> Notifications, int TotalCount)> GetNotificationsAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<int> GetUnreadNotificationCountAsync(Guid userId);
        Task MarkNotificationAsReadAsync(Guid notificationId, Guid userId);
        Task MarkAllNotificationsAsReadAsync(Guid userId);
        Task DeleteNotificationAsync(Guid notificationId, Guid userId);
    }
} 
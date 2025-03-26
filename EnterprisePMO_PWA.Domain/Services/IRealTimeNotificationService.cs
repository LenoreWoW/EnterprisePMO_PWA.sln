using System;
using System.Threading.Tasks;

namespace EnterprisePMO_PWA.Domain.Services
{
    /// <summary>
    /// Defines methods for real-time notifications.
    /// </summary>
    public interface IRealTimeNotificationService
    {
        /// <summary>
        /// Sends a real-time notification to a specific user.
        /// </summary>
        Task SendToUserAsync(Guid userId, string message);

        /// <summary>
        /// Broadcasts a notification to all connected clients.
        /// </summary>
        Task BroadcastAsync(string message);
    }
}
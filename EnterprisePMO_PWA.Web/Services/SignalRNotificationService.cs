using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using EnterprisePMO_PWA.Domain.Services;
using EnterprisePMO_PWA.Web.Hubs;

namespace EnterprisePMO_PWA.Web.Services
{
    /// <summary>
    /// Provides real-time notifications using SignalR.
    /// </summary>
    public class SignalRNotificationService : IRealTimeNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<SignalRNotificationService> _logger;

        public SignalRNotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<SignalRNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Sends a real-time notification to a specific user.
        /// </summary>
        public async Task SendToUserAsync(Guid userId, string message)
        {
            try
            {
                // In SignalR, we can send to a specific connection or user
                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync("ReceiveNotification", new
                    {
                        message,
                        timestamp = DateTime.UtcNow
                    });
                
                _logger.LogInformation($"Sent notification to user {userId}: {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notification to user {userId}");
                // Don't rethrow - notifications are best-effort
            }
        }

        /// <summary>
        /// Broadcasts a notification to all connected clients.
        /// </summary>
        public async Task BroadcastAsync(string message)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
                {
                    message,
                    timestamp = DateTime.UtcNow
                });
                
                _logger.LogInformation($"Broadcast notification: {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting notification");
                // Don't rethrow - notifications are best-effort
            }
        }
    }
}
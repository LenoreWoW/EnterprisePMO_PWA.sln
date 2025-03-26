using System;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Services;
using EnterprisePMO_PWA.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EnterprisePMO_PWA.Web.Services
{
    /// <summary>
    /// Implements real-time notifications using SignalR.
    /// </summary>
    public class SignalRNotificationService : IRealTimeNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// Sends a real-time notification to a specific user.
        /// </summary>
        public async Task SendToUserAsync(Guid userId, string message)
        {
            await _hubContext.Clients.Group(userId.ToString())
                .SendAsync("ReceiveNotification", message);
        }

        /// <summary>
        /// Broadcasts a notification to all connected clients.
        /// </summary>
        public async Task BroadcastAsync(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
        }
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EnterprisePMO_PWA.Web.Hubs
{
    /// <summary>
    /// SignalR hub for real-time notifications.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Called when a client connects to the hub.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (!string.IsNullOrEmpty(userId))
                {
                    // Add the connection to the user's group
                    await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                    _logger.LogInformation($"User {userId} connected to notification hub");
                }
                else
                {
                    _logger.LogWarning("User connected without valid ID");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync");
            }
            
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a client disconnects from the hub.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (!string.IsNullOrEmpty(userId))
                {
                    // Remove the connection from the user's group
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                    _logger.LogInformation($"User {userId} disconnected from notification hub");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync");
            }
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}
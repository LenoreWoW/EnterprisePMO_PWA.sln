using System;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Domain.Entities;

namespace EnterprisePMO_PWA.Web.Controllers
{
    /// <summary>
    /// API controller for managing user notifications.
    /// </summary>
    [Route("api/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly EnhancedNotificationService _notificationService;

        public NotificationsController(EnhancedNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Gets all notifications for the current user with pagination.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                var (notifications, totalCount) = await _notificationService.GetNotificationsAsync(
                    userId.Value, page, pageSize);

                return Ok(new
                {
                    success = true,
                    data = notifications,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving notifications",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets unread notifications for the current user.
        /// </summary>
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                var notifications = await _notificationService.GetUnreadNotificationsAsync(userId.Value);

                return Ok(new
                {
                    success = true,
                    data = notifications
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving unread notifications",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets the count of unread notifications for the current user.
        /// </summary>
        [HttpGet("unread/count")]
        public async Task<IActionResult> GetUnreadNotificationCount()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                var count = await _notificationService.GetUnreadNotificationCountAsync(userId.Value);

                return Ok(new
                {
                    success = true,
                    count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving unread notification count",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Marks a notification as read.
        /// </summary>
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                await _notificationService.MarkNotificationAsReadAsync(id, userId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Notification marked as read"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while marking notification as read",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Marks all notifications as read for the current user.
        /// </summary>
        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                await _notificationService.MarkAllNotificationsAsReadAsync(userId.Value);

                return Ok(new
                {
                    success = true,
                    message = "All notifications marked as read"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while marking all notifications as read",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a notification.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                await _notificationService.DeleteNotificationAsync(id, userId.Value);

                return Ok(new
                {
                    success = true,
                    message = "Notification deleted"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting notification",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Background sync endpoint for offline support.
        /// </summary>
        [HttpGet("sync")]
        public IActionResult SyncNotifications()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            // In a real implementation, this would handle syncing notifications
            // for offline clients, but for this demo we'll just return success
            return Ok(new
            {
                success = true,
                message = "Notifications synced successfully"
            });
        }

        /// <summary>
        /// Gets the current user ID from claims.
        /// </summary>
        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }
            
            return null;
        }
    }
}
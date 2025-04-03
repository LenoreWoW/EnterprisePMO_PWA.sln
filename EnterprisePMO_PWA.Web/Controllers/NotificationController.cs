using System;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;
        private readonly PushNotificationService _pushNotificationService;
        private readonly ILogger<NotificationController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public NotificationController(
            NotificationService notificationService,
            PushNotificationService pushNotificationService,
            ILogger<NotificationController> logger,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _notificationService = notificationService;
            _pushNotificationService = pushNotificationService;
            _logger = logger;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionRequest request)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                await _pushNotificationService.SaveSubscriptionAsync(
                    userId,
                    request.Endpoint,
                    request.P256dh,
                    request.Auth);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to push notifications");
                return StatusCode(500, "Error subscribing to push notifications");
            }
        }

        [HttpPost("unsubscribe")]
        public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeRequest request)
        {
            try
            {
                await _pushNotificationService.DeleteSubscriptionAsync(request.Endpoint);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing from push notifications");
                return StatusCode(500, "Error unsubscribing from push notifications");
            }
        }

        [HttpGet("preferences")]
        public async Task<IActionResult> GetPreferences()
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var preferences = await _notificationService.GetUserNotificationPreferencesAsync(userId);
                return Ok(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification preferences");
                return StatusCode(500, "Error getting notification preferences");
            }
        }

        [HttpPut("preferences")]
        public async Task<IActionResult> UpdatePreferences([FromBody] UserNotificationPreferences preferences)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                preferences.UserId = userId;
                await _notificationService.UpdateUserNotificationPreferencesAsync(preferences);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification preferences");
                return StatusCode(500, "Error updating notification preferences");
            }
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var notifications = await _notificationService.GetUserNotificationsAsync(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return StatusCode(500, "Error getting notifications");
            }
        }

        [HttpPut("notifications/{id}/read")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                await _notificationService.MarkNotificationAsReadAsync(id, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return StatusCode(500, "Error marking notification as read");
            }
        }

        [HttpGet("vapid-public-key")]
        public IActionResult GetVapidPublicKey()
        {
            try
            {
                var publicKey = _configuration["PushNotifications:VapidPublicKey"];
                if (string.IsNullOrEmpty(publicKey))
                {
                    return StatusCode(500, "VAPID public key not configured");
                }
                return Ok(new { publicKey });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting VAPID public key");
                return StatusCode(500, "Error getting VAPID public key");
            }
        }

        [HttpPost("test-email")]
        [Authorize]
        public async Task<IActionResult> TestEmail()
        {
            try
            {
                var user = User;
                var userEmail = user.FindFirst(ClaimTypes.Email)?.Value;
                
                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest("User email not found");
                }

                await _emailService.SendEmailAsync(
                    userEmail,
                    "Test Email from Enterprise PMO",
                    "<h1>Test Email</h1><p>This is a test email to verify the email configuration is working correctly.</p>"
                );

                return Ok("Test email sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test email");
                return StatusCode(500, "Error sending test email: " + ex.Message);
            }
        }

        [HttpPost("subscribe/{userId}/{projectId}")]
        public async Task<IActionResult> SubscribeToProject(Guid userId, Guid projectId)
        {
            await _notificationService.SubscribeToProjectAsync(userId, projectId);
            return Ok();
        }
    }

    public class PushSubscriptionRequest
    {
        public string Endpoint { get; set; }
        public string P256dh { get; set; }
        public string Auth { get; set; }
    }

    public class UnsubscribeRequest
    {
        public string Endpoint { get; set; }
    }
} 
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Configuration;
using EnterprisePMO_PWA.Domain.Services;

namespace EnterprisePMO_PWA.Infrastructure.Services
{
    /// <summary>
    /// Implementation of the real-time notification service using a web service.
    /// </summary>
    public class RealTimeNotificationService : IRealTimeNotificationService
    {
        private readonly string _notificationEndpoint;
        private readonly HttpClient _httpClient;

        public RealTimeNotificationService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            
            // Get the notification endpoint from configuration
            var endpoint = configuration["Notifications:RealTimeEndpoint"];
            if (string.IsNullOrEmpty(endpoint))
            {
                endpoint = "https://api.example.com/notifications"; // Fallback URL
                Console.WriteLine("Warning: RealTime notification endpoint not configured. Using default.");
            }
            
            _notificationEndpoint = endpoint;
        }

        /// <summary>
        /// Sends a real-time notification to a specific user.
        /// </summary>
        public async Task SendToUserAsync(Guid userId, string message)
        {
            try
            {
                var payload = new
                {
                    userId = userId.ToString(),
                    message = message,
                    timestamp = DateTime.UtcNow
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_notificationEndpoint}/user", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                // Log but don't throw - real-time notifications are best-effort
                Console.WriteLine($"Error sending real-time notification to user {userId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Broadcasts a notification to all connected clients.
        /// </summary>
        public async Task BroadcastAsync(string message)
        {
            try
            {
                var payload = new
                {
                    message = message,
                    timestamp = DateTime.UtcNow
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_notificationEndpoint}/broadcast", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                // Log but don't throw - real-time notifications are best-effort
                Console.WriteLine($"Error broadcasting real-time notification: {ex.Message}");
            }
        }
    }
}
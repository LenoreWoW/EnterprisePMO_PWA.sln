using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Supabase;
using Supabase.Postgrest;
using Supabase.Postgrest.Attributes;
using static Supabase.Postgrest.Constants;
using Supabase.Postgrest.Models;
using EnterprisePMO_PWA.Domain.Interfaces;

namespace EnterprisePMO_PWA.Application.Services
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly ILogger<PushNotificationService> _logger;
        private readonly IConfiguration _configuration;

        public PushNotificationService(
            Supabase.Client supabaseClient,
            ILogger<PushNotificationService> logger,
            IConfiguration configuration)
        {
            _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Sends a push notification to a user
        /// </summary>
        public async Task SendPushNotificationAsync(string userId, string title, string message, string? url = null)
        {
            try
            {
                // Get user's push notification subscription
                var response = await _supabaseClient
                    .From<PushSubscription>()
                    .Filter("user_id", Operator.Equals, userId)
                    .Single();

                if (response == null)
                {
                    _logger.LogWarning($"No push subscription found for user {userId}");
                    return;
                }

                var subscription = response;

                // Here you would implement the actual push notification sending logic
                // This could use a service like Firebase Cloud Messaging, OneSignal, etc.
                // For now, we'll just log it
                _logger.LogInformation($"Sending push notification to user {userId}: {title} - {message}");

                // Example implementation with a hypothetical push notification service:
                // await _pushNotificationService.SendAsync(subscription.Endpoint, subscription.P256dh, subscription.Auth, title, message, url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification to user {UserId}", userId);
            }
        }

        /// <summary>
        /// Sends a push notification to a specific subscription
        /// </summary>
        public async Task SendPushNotificationToSubscription(string endpoint, string p256dh, string auth, string title, string message, string? url = null)
        {
            try
            {
                // Here you would implement the actual push notification sending logic
                // This could use a service like Firebase Cloud Messaging, OneSignal, etc.
                // For now, we'll just log it
                _logger.LogInformation($"Sending push notification to subscription {endpoint}: {title} - {message}");

                // Example implementation with a hypothetical push notification service:
                // await _pushNotificationService.SendAsync(endpoint, p256dh, auth, title, message, url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification to subscription {Endpoint}", endpoint);
            }
        }

        /// <summary>
        /// Saves a push notification subscription
        /// </summary>
        public async Task<bool> SaveSubscriptionAsync(string userId, string endpoint, string p256dh, string auth)
        {
            try
            {
                var subscription = new PushSubscription
                {
                    UserId = userId,
                    Endpoint = endpoint,
                    P256dh = p256dh,
                    Auth = auth,
                    LastUsed = DateTime.UtcNow
                };

                var response = await _supabaseClient
                    .From<PushSubscription>()
                    .Insert(subscription);

                return response.Models.Count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving push subscription for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Deletes a push notification subscription
        /// </summary>
        public async Task<bool> DeleteSubscriptionAsync(string endpoint)
        {
            try
            {
                await _supabaseClient
                    .From<PushSubscription>()
                    .Filter("endpoint", Operator.Equals, endpoint)
                    .Delete();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting push subscription with endpoint {Endpoint}", endpoint);
                return false;
            }
        }

        /// <summary>
        /// Gets a push notification subscription for a user and endpoint
        /// </summary>
        public async Task<PushSubscription> GetSubscriptionAsync(string userId, string endpoint)
        {
            try
            {
                var response = await _supabaseClient
                    .From<PushSubscription>()
                    .Filter("user_id", Operator.Equals, userId)
                    .Filter("endpoint", Operator.Equals, endpoint)
                    .Single();

                if (response == null)
                {
                    throw new InvalidOperationException($"No push subscription found for user {userId} and endpoint {endpoint}");
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving push subscription for user {UserId} and endpoint {Endpoint}", userId, endpoint);
                throw;
            }
        }

        /// <summary>
        /// Updates the last used timestamp for a subscription
        /// </summary>
        public async Task<bool> UpdateLastUsedAsync(string endpoint)
        {
            try
            {
                var subscription = new PushSubscription
                {
                    Endpoint = endpoint,
                    LastUsed = DateTime.UtcNow
                };

                await _supabaseClient
                    .From<PushSubscription>()
                    .Filter("endpoint", Operator.Equals, endpoint)
                    .Update(subscription);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last used timestamp for subscription with endpoint {Endpoint}", endpoint);
                return false;
            }
        }
    }
} 
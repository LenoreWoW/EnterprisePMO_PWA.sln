using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;

namespace EnterprisePMO_PWA.Domain.Interfaces
{
    public interface IPushNotificationService
    {
        Task<PushSubscription> GetSubscriptionAsync(string userId, string endpoint);
        Task<bool> SaveSubscriptionAsync(string userId, string endpoint, string p256dh, string auth);
        Task<bool> DeleteSubscriptionAsync(string endpoint);
    }
} 
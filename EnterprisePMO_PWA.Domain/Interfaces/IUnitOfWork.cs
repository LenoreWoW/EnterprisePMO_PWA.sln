using System;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;

namespace EnterprisePMO_PWA.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<UserNotificationPreferences> UserNotificationPreferences { get; }
        IRepository<ProjectSubscription> ProjectSubscriptions { get; }
        IRepository<Notification> Notifications { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
} 
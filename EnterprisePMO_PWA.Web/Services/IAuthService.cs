using System;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;

namespace EnterprisePMO_PWA.Web.Services
{
    public interface IAuthService
    {
        Task<bool> SignInAsync(string email, string password);
        Task SignOutAsync();
        Task<User> GetCurrentUserAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<bool> HasPermissionAsync(string permission);
        Task<bool> HasAnyPermissionAsync(params string[] permissions);
        Task<bool> HasAllPermissionsAsync(params string[] permissions);
    }
} 
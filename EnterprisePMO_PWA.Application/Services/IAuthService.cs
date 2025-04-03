using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Enums;

namespace EnterprisePMO_PWA.Application.Services
{
    public interface IAuthService
    {
        Task<bool> SignInAsync(string email, string password);
        Task SignOutAsync();
        Task<User> GetCurrentUserAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<bool> HasPermissionAsync(string permission);
        Task<bool> HasAnyPermissionAsync(IEnumerable<string> permissions);
        Task<bool> HasAllPermissionsAsync(IEnumerable<string> permissions);
        Task<AuthResult> LoginAsync(string email, string password);
        Task<AuthResult> SignupAsync(SignupRequest request);
        Task<bool> LogoutAsync(string token);
        Task<AuthResult> RefreshTokenAsync(string refreshToken);
        Task<PasswordResetResult> ResetPasswordAsync(string email);
        Task<AuthResult> VerifyTokenAsync(string token);
        Task<User> GetUserBySupabaseIdAsync(string supabaseId);
        Task<User> SyncUserWithSupabaseAsync(string email, string supabaseId);
        Task LogActionAsync(string entityType, string entityId, string action, string details);
        Task LogActionAsync(string entityType, Guid entityId, string action, string details);
        Task LogActionAsync(string entityType, string action, string details);
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public User? User { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Message { get; set; }
        public int ExpiresIn { get; set; }
    }

    public class PasswordResetResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public class SignupRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool TermsAgreed { get; set; }
        public Guid? DepartmentId { get; set; }
        public UserRole Role { get; set; } = UserRole.ProjectManager;
    }
}
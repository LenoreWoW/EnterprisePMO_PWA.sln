using System;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;

namespace EnterprisePMO_PWA.Application.Services
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(string email, string password);
        Task<AuthResult> SignupAsync(SignupRequest request);
        Task<bool> LogoutAsync(string token);
        Task<AuthResult> RefreshTokenAsync(string refreshToken);
        Task<PasswordResetResult> ResetPasswordAsync(string email);
        Task<AuthResult> VerifyTokenAsync(string token);
        Task<User> GetUserBySupabaseIdAsync(string supabaseId);
        Task<User> SyncUserWithSupabaseAsync(string email, string supabaseId);
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
    }
}
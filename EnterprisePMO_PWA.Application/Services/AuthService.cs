using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Enums;
using EnterprisePMO_PWA.Infrastructure.Services;
using EnterprisePMO_PWA.Domain.Authorization;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EnterprisePMO_PWA.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly SupabaseClient _supabaseClient;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly AuditService _auditService;
        private string? _currentToken;
        private User? _currentUser;
        private readonly AppDbContext _context;
        private readonly IPermissionService _permissionService;

        public AuthService(
            SupabaseClient supabaseClient,
            ILogger<AuthService> logger,
            IConfiguration configuration,
            AuditService auditService,
            AppDbContext context,
            IPermissionService permissionService)
        {
            _supabaseClient = supabaseClient;
            _logger = logger;
            _configuration = configuration;
            _auditService = auditService;
            _context = context;
            _permissionService = permissionService;
        }

        public async Task<bool> SignInAsync(string email, string password)
        {
            try
            {
                var result = await LoginAsync(email, password);
                if (result.Success && result.Token != null)
                {
                    _currentToken = result.Token;
                    _currentUser = result.User;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sign in");
                return false;
            }
        }

        public async Task SignOutAsync()
        {
            try
            {
                if (_currentToken != null)
                {
                    await LogoutAsync(_currentToken);
                    _currentToken = null;
                    _currentUser = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sign out");
            }
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            try
            {
                if (_currentUser != null)
                    return _currentUser;

                if (_currentToken != null)
                {
                    var userResponse = await _supabaseClient.Auth.GetUser(_currentToken);
                    if (userResponse.User?.Id != null)
                    {
                        _currentUser = await GetUserBySupabaseIdAsync(userResponse.User.Id);
                        return _currentUser;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return null;
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                if (_currentToken == null)
                    return false;

                var userResponse = await _supabaseClient.Auth.GetUser(_currentToken);
                return userResponse.User != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking authentication status");
                return false;
            }
        }

        public async Task<bool> HasPermissionAsync(string permission)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return false;

                return await _permissionService.HasPermissionAsync(user.Id, permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission");
                return false;
            }
        }

        public async Task<bool> HasAnyPermissionAsync(IEnumerable<string> permissions)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return false;

                return await _permissionService.HasAnyPermissionAsync(user.Id, permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permissions");
                return false;
            }
        }

        public async Task<bool> HasAllPermissionsAsync(IEnumerable<string> permissions)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return false;

                return await _permissionService.HasAllPermissionsAsync(user.Id, permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permissions");
                return false;
            }
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                // Authenticate with Supabase
                var authResponse = await _supabaseClient.Auth.SignInWithPassword(email, password);

                if (authResponse.Error != null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = authResponse.Error.Message
                    };
                }

                // Get user profile from Supabase database
                var user = await _supabaseClient.Database.SelectAsync<User>(
                    "users",
                    authResponse.Session?.AccessToken,
                    $"email=eq.{email}");

                if (user == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = "User profile not found"
                    };
                }

                // Log successful login
                await _auditService.LogActionAsync(
                    "Authentication",
                    user.Id,
                    "Login",
                    "User logged in successfully"
                );

                return new AuthResult
                {
                    Success = true,
                    Token = authResponse.Session?.AccessToken,
                    RefreshToken = authResponse.Session?.RefreshToken,
                    User = user,
                    ExpiresIn = authResponse.Session?.ExpiresIn ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "An error occurred during login"
                };
            }
        }

        public async Task<AuthResult> SignupAsync(SignupRequest request)
        {
            try
            {
                // Check if user exists in Supabase
                var existingUser = await _supabaseClient.Database.SelectAsync<User>(
                    "users",
                    null,
                    $"email=eq.{request.Email}");

                if (existingUser != null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = "User already exists"
                    };
                }

                // Create new user in Supabase
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    FullName = request.FirstName + " " + request.LastName,
                    Username = request.Email,
                    Role = request.Role,
                    DepartmentId = request.DepartmentId ?? Guid.Empty,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _supabaseClient.Database.InsertAsync<User>("users", user);

                // Log successful signup
                await _auditService.LogActionAsync(
                    "Authentication",
                    user.Id,
                    "Signup",
                    "User signed up successfully"
                );

                return new AuthResult
                {
                    Success = true,
                    User = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during signup");
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "An error occurred during signup"
                };
            }
        }

        private int GetRoleHierarchyLevel(UserRole roleType)
        {
            switch (roleType)
            {
                case UserRole.Admin:
                    return 100;
                case UserRole.PMOHead:
                    return 90;
                case UserRole.ProjectManager:
                    return 80;
                case UserRole.TeamMember:
                    return 70;
                case UserRole.Viewer:
                    return 60;
                default:
                    return 10;
            }
        }

        public async Task<bool> LogoutAsync(string token)
        {
            try
            {
                var response = await _supabaseClient.Auth.SignOut(token);
                return response.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return false;
            }
        }

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var response = await _supabaseClient.Auth.RefreshSession(refreshToken);

                if (response.Error != null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = response.Error.Message
                    };
                }

                // Get user profile from Supabase database
                var user = await _supabaseClient.Database.SelectAsync<User>(
                    "users",
                    response.Session?.AccessToken,
                    $"supabase_id=eq.{response.User?.Id}");

                if (user == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = "User profile not found"
                    };
                }

                return new AuthResult
                {
                    Success = true,
                    Token = response.Session?.AccessToken,
                    RefreshToken = response.Session?.RefreshToken,
                    User = user,
                    ExpiresIn = response.Session?.ExpiresIn ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "An error occurred while refreshing the token"
                };
            }
        }

        public async Task<PasswordResetResult> ResetPasswordAsync(string email)
        {
            try
            {
                string redirectUrl = _configuration["AppUrl"] + "/account/reset-password";
                
                var response = await _supabaseClient.Auth.ResetPasswordForEmail(
                    email,
                    new ResetPasswordForEmailOptions
                    {
                        RedirectTo = redirectUrl
                    });

                if (response.Error != null)
                {
                    _logger.LogWarning($"Password reset failed for {email}: {response.Error.Message}");
                }

                // Always return success to prevent email enumeration
                return new PasswordResetResult
                {
                    Success = true,
                    Message = "If your email is registered, you will receive a password reset link"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in password reset for {email}");
                return new PasswordResetResult
                {
                    Success = true,
                    Message = "If your email is registered, you will receive a password reset link"
                };
            }
        }

        public async Task<AuthResult> VerifyTokenAsync(string token)
        {
            try
            {
                var user = await _supabaseClient.Auth.GetUser(token);

                if (user.Error != null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = user.Error.Message
                    };
                }

                // Get user profile from Supabase database
                var appUser = await _supabaseClient.Database.SelectAsync<User>(
                    "users",
                    token,
                    $"supabase_id=eq.{user.User?.Id}");

                if (appUser == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = "User not found in application database"
                    };
                }

                return new AuthResult
                {
                    Success = true,
                    User = appUser
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying token");
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "An error occurred while verifying the token"
                };
            }
        }

        public async Task<User> GetUserBySupabaseIdAsync(string supabaseId)
        {
            return await _supabaseClient.Database.SelectAsync<User>(
                "users",
                null,
                $"supabase_id=eq.{supabaseId}");
        }

        public async Task<User> SyncUserWithSupabaseAsync(string email, string supabaseId)
        {
            // Check if user exists in Supabase database
            var user = await _supabaseClient.Database.SelectAsync<User>(
                "users",
                null,
                $"email=eq.{email}");

            if (user != null)
            {
                // Update Supabase ID if not set
                if (string.IsNullOrEmpty(user.SupabaseId))
                {
                    user.SupabaseId = supabaseId;
                    await _supabaseClient.Database.UpdateAsync<User>(
                        "users",
                        new { supabase_id = supabaseId },
                        $"id=eq.{user.Id}");
                }
                return user;
            }

            // Get holding department
            var holdingDepartment = await _supabaseClient.Database.SelectAsync<Department>(
                "departments",
                null,
                "name=eq.Holding");

            if (holdingDepartment == null)
            {
                // Create holding department
                holdingDepartment = new Department
                {
                    Id = Guid.NewGuid(),
                    Name = "Holding"
                };
                await _supabaseClient.Database.InsertAsync<Department>(
                    "departments",
                    holdingDepartment);
            }

            // Create new user
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = email,
                SupabaseId = supabaseId,
                Role = UserRole.ProjectManager,
                DepartmentId = holdingDepartment.Id
            };

            var createdUser = await _supabaseClient.Database.InsertAsync<User>(
                "users",
                newUser);

            // Log the user creation
            await _auditService.LogActionAsync(
                "Authentication",
                createdUser.Id,
                "UserCreation",
                "User automatically created during authentication sync"
            );

            return createdUser;
        }

        public async Task LogActionAsync(string entityType, string entityId, string action, string details)
        {
            await _auditService.LogActionAsync(entityType, entityId, action, details);
        }

        public async Task LogActionAsync(string entityType, Guid entityId, string action, string details)
        {
            await _auditService.LogActionAsync(entityType, entityId, action, details);
        }

        public async Task LogActionAsync(string entityType, string action, string details)
        {
            await _auditService.LogActionAsync(entityType, "System", action, details);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Infrastructure.Data;
using EnterprisePMO_PWA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnterprisePMO_PWA.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly SupabaseClient _supabaseClient;
        private readonly AppDbContext _dbContext;
        private readonly AuditService _auditService;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;

        public AuthService(
            SupabaseClient supabaseClient, 
            AppDbContext dbContext,
            AuditService auditService,
            ILogger<AuthService> logger,
            IConfiguration configuration)
        {
            _supabaseClient = supabaseClient;
            _dbContext = dbContext;
            _auditService = auditService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                // Authenticate with Supabase
                var response = await _supabaseClient.Auth.SignInWithPassword(email, password);

                if (response.Error != null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = response.Error.Message
                    };
                }

                // Get or create user in our database
                var user = await GetUserByEmailAsync(email);
                if (user == null)
                {
                    // First-time login with Supabase - create user
                    user = await SyncUserWithSupabaseAsync(email, response.User.Id);
                }
                else if (string.IsNullOrEmpty(user.SupabaseId))
                {
                    // Update existing user with Supabase ID
                    user.SupabaseId = response.User.Id;
                    await _dbContext.SaveChangesAsync();
                }

                // Log the successful login
                await _auditService.LogActionAsync(
                    "Authentication",
                    user.Id,
                    "Login",
                    "User logged in successfully via Supabase"
                );

                return new AuthResult
                {
                    Success = true,
                    Token = response.Session.AccessToken,
                    RefreshToken = response.Session.RefreshToken,
                    User = user,
                    ExpiresIn = response.Session.ExpiresIn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "An unexpected error occurred during login."
                };
            }
        }

        public async Task<AuthResult> SignupAsync(SignupRequest request)
        {
            try
            {
                // Check if user already exists in our database
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Email);

                if (existingUser != null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = "A user with this email already exists"
                    };
                }

                // Register with Supabase
                var signUpOptions = new SignUpOptions
                {
                    Data = new Dictionary<string, object>
                    {
                        { "first_name", request.FirstName },
                        { "last_name", request.LastName }
                    }
                };

                var response = await _supabaseClient.Auth.SignUp(request.Email, request.Password, signUpOptions);

                if (response.Error != null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = response.Error.Message
                    };
                }

                // Create user in our database
                var holdingDepartment = await _dbContext.Departments
                    .FirstOrDefaultAsync(d => d.Name == "Holding");

                if (holdingDepartment == null)
                {
                    // Create a holding department if it doesn't exist
                    holdingDepartment = new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = "Holding"
                    };
                    _dbContext.Departments.Add(holdingDepartment);
                    await _dbContext.SaveChangesAsync();
                }

                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = request.Email,
                    Role = RoleType.ProjectManager, // Default role
                    DepartmentId = request.DepartmentId ?? holdingDepartment.Id,
                    SupabaseId = response.User.Id
                };

                _dbContext.Users.Add(newUser);
                await _dbContext.SaveChangesAsync();

                // Log the registration
                await _auditService.LogActionAsync(
                    "Authentication",
                    newUser.Id,
                    "Registration",
                    "New user registered successfully"
                );

                // Return the result
                return new AuthResult
                {
                    Success = true,
                    Message = "Account created successfully",
                    Token = response.Session?.AccessToken,
                    RefreshToken = response.Session?.RefreshToken,
                    User = newUser,
                    ExpiresIn = response.Session?.ExpiresIn ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during signup");
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "An unexpected error occurred during registration."
                };
            }
        }

        public async Task<bool> LogoutAsync(string token)
        {
            try
            {
                // Logout from Supabase
                await _supabaseClient.Auth.SignOut();
                return true;
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
                // Call Supabase to refresh the token
                var response = await _supabaseClient.Auth.RefreshSession(refreshToken);

                if (response.Error != null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = response.Error.Message
                    };
                }

                // Get user from our database
                var user = await GetUserBySupabaseIdAsync(response.User.Id);

                return new AuthResult
                {
                    Success = true,
                    Token = response.Session.AccessToken,
                    RefreshToken = response.Session.RefreshToken,
                    User = user,
                    ExpiresIn = response.Session.ExpiresIn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "An error occurred while refreshing the token."
                };
            }
        }

        public async Task<PasswordResetResult> ResetPasswordAsync(string email)
        {
            try
            {
                string redirectUrl = _configuration["AppUrl"] + "/account/reset-password";
                
                // Call Supabase to send reset password email
                var response = await _supabaseClient.Auth.ResetPasswordForEmail(
                    email, 
                    new ResetPasswordForEmailOptions
                    {
                        RedirectTo = redirectUrl
                    });

                if (response.Error != null)
                {
                    // Log the error but always return success to prevent email enumeration
                    _logger.LogWarning($"Password reset failed for {email}: {response.Error.Message}");
                }

                // Always return success to prevent email enumeration attacks
                return new PasswordResetResult
                {
                    Success = true,
                    Message = "If your email is registered, you will receive a password reset link."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in password reset for {email}");
                // Always return success to prevent email enumeration attacks
                return new PasswordResetResult
                {
                    Success = true,
                    Message = "If your email is registered, you will receive a password reset link."
                };
            }
        }

        public async Task<AuthResult> VerifyTokenAsync(string token)
        {
            try
            {
                // Verify token with Supabase
                var user = await _supabaseClient.Auth.GetUser(token);

                if (user.Error != null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = user.Error.Message
                    };
                }

                // Get user from our database
                var appUser = await GetUserBySupabaseIdAsync(user.User.Id);
                
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
                    ErrorMessage = "An error occurred while verifying the token."
                };
            }
        }

        public async Task<User> GetUserBySupabaseIdAsync(string supabaseId)
        {
            return await _dbContext.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.SupabaseId == supabaseId);
        }

        private async Task<User> GetUserByEmailAsync(string email)
        {
            return await _dbContext.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Username == email);
        }

        public async Task<User> SyncUserWithSupabaseAsync(string email, string supabaseId)
        {
            // Check if user exists in our database
            var user = await _dbContext.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Username == email);

            if (user != null)
            {
                // Update Supabase ID if not set
                if (string.IsNullOrEmpty(user.SupabaseId))
                {
                    user.SupabaseId = supabaseId;
                    await _dbContext.SaveChangesAsync();
                }
                return user;
            }

            // User doesn't exist, create a new one
            var holdingDepartment = await _dbContext.Departments
                .FirstOrDefaultAsync(d => d.Name == "Holding");

            if (holdingDepartment == null)
            {
                // Create a holding department if it doesn't exist
                holdingDepartment = new Department
                {
                    Id = Guid.NewGuid(),
                    Name = "Holding"
                };
                _dbContext.Departments.Add(holdingDepartment);
                await _dbContext.SaveChangesAsync();
            }

            // Create new user
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = email,
                SupabaseId = supabaseId,
                Role = RoleType.ProjectManager, // Default role
                DepartmentId = holdingDepartment.Id
            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            // Log the user creation
            await _auditService.LogActionAsync(
                "Authentication",
                newUser.Id,
                "UserCreation",
                "User automatically created during authentication sync"
            );

            return newUser;
        }
    }
}
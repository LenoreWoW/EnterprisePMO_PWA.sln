// EnterprisePMO_PWA.Web/Services/SupabaseAuthService.cs

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace EnterprisePMO_PWA.Web.Services
{
    /// <summary>
    /// Consolidated authentication service that uses Supabase for authentication
    /// and manages application user state and roles.
    /// </summary>
    public class SupabaseAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly string _supabaseUrl;
        private readonly string _supabaseKey;
        private readonly string _jwtSecret;
        
        public SupabaseAuthService(
            HttpClient httpClient,
            AppDbContext dbContext,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _configuration = configuration;
            
            // Get Supabase configuration
            var projectRef = configuration["Supabase:ProjectRef"];
            _supabaseUrl = !string.IsNullOrEmpty(projectRef) 
                ? $"https://{projectRef}.supabase.co" 
                : "https://example.supabase.co";
                
            _supabaseKey = configuration["Supabase:AnonKey"] ?? "default-key-for-dev";
            _jwtSecret = configuration["Jwt:SecretKey"] ?? "your-development-fallback-key-with-at-least-32-chars";
            
            // Configure HTTP client for Supabase requests
            _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
        }

        /// <summary>
        /// Handles user login by authenticating with Supabase and retrieving application user info
        /// </summary>
        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                // Check for test admin account first for development
                if (email == "admin@test.com" && password == "Password123!")
                {
                    return await HandleTestAdminLoginAsync();
                }
                
                // 1. Authenticate with Supabase
                var supabaseAuthRequest = new 
                {
                    email,
                    password
                };
                
                var authResponse = await _httpClient.PostAsJsonAsync(
                    $"{_supabaseUrl}/auth/v1/token?grant_type=password",
                    supabaseAuthRequest);
                
                if (!authResponse.IsSuccessStatusCode)
                {
                    var errorContent = await authResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Supabase auth failed: {errorContent}");
                }
                
                var authResult = await authResponse.Content.ReadFromJsonAsync<SupabaseAuthResponse>();
                if (authResult == null || string.IsNullOrEmpty(authResult.AccessToken))
                {
                    throw new Exception("Invalid response from authentication service");
                }
                
                // 2. Get user info from our application database
                var user = await _dbContext.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Username == email);
                
                if (user == null)
                {
                    throw new Exception("User not found in application database");
                }
                
                // 3. Generate application JWT with roles and permissions
                var appToken = GenerateApplicationJwt(user);
                
                // Return combined auth result
                return new AuthResult
                {
                    Success = true,
                    Token = appToken,
                    RefreshToken = authResult.RefreshToken,
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Role = user.Role.ToString(),
                        DepartmentId = user.DepartmentId,
                        DepartmentName = user.Department?.Name
                    }
                };
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.Error.WriteLine($"Authentication error: {ex.Message}");
                
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Authentication failed: " + ex.Message
                };
            }
        }
        
        /// <summary>
        /// Creates a new user account in both Supabase and our application database
        /// </summary>
        public async Task<AuthResult> SignupAsync(SignupRequest request)
        {
            try
            {
                // Validate request
                if (request.Password != request.ConfirmPassword)
                {
                    throw new Exception("Passwords do not match");
                }
                
                // Check if user already exists in our database
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Email);
                
                if (existingUser != null)
                {
                    throw new Exception("User with this email already exists");
                }
                
                // 1. Create user in Supabase
                var supabaseSignupRequest = new 
                {
                    email = request.Email,
                    password = request.Password
                };
                
                var signupResponse = await _httpClient.PostAsJsonAsync(
                    $"{_supabaseUrl}/auth/v1/signup",
                    supabaseSignupRequest);
                
                if (!signupResponse.IsSuccessStatusCode)
                {
                    var errorContent = await signupResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Supabase signup failed: {errorContent}");
                }
                
                var supabaseUser = await signupResponse.Content.ReadFromJsonAsync<SupabaseUser>();
                
                // 2. Create user in our application database
                var department = await _dbContext.Departments.FindAsync(request.DepartmentId);
                if (department == null)
                {
                    // Default to "Holding" department if specified department not found
                    department = await _dbContext.Departments
                        .FirstOrDefaultAsync(d => d.Name == "Holding");
                        
                    if (department == null)
                    {
                        throw new Exception("No valid department found for new user");
                    }
                }
                
                // Create new application user
                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = request.Email,
                    Role = RoleType.ProjectManager, // Default role for new users
                    DepartmentId = department.Id,
                    SupabaseId = supabaseUser?.Id
                };
                
                _dbContext.Users.Add(newUser);
                await _dbContext.SaveChangesAsync();
                
                // 3. Generate application JWT with roles and permissions
                var appToken = GenerateApplicationJwt(newUser);
                
                // Return successful result
                return new AuthResult
                {
                    Success = true,
                    Token = appToken, 
                    User = new UserInfo
                    {
                        Id = newUser.Id,
                        Username = newUser.Username,
                        Role = newUser.Role.ToString(),
                        DepartmentId = newUser.DepartmentId,
                        DepartmentName = department.Name
                    },
                    Message = "Account created successfully"
                };
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.Error.WriteLine($"Signup error: {ex.Message}");
                
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Signup failed: " + ex.Message
                };
            }
        }
        
        /// <summary>
        /// Refreshes the authentication token
        /// </summary>
        public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // 1. Refresh Supabase token
                var refreshRequest = new 
                {
                    refresh_token = refreshToken
                };
                
                var refreshResponse = await _httpClient.PostAsJsonAsync(
                    $"{_supabaseUrl}/auth/v1/token?grant_type=refresh_token",
                    refreshRequest);
                
                if (!refreshResponse.IsSuccessStatusCode)
                {
                    var errorContent = await refreshResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Token refresh failed: {errorContent}");
                }
                
                var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<SupabaseAuthResponse>();
                if (refreshResult == null || string.IsNullOrEmpty(refreshResult.AccessToken))
                {
                    throw new Exception("Invalid response from refresh token service");
                }
                
                // 2. Get user info from the token
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(refreshResult.AccessToken) as JwtSecurityToken;
                
                if (jsonToken == null)
                {
                    throw new Exception("Invalid token format");
                }
                
                var emailClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                if (string.IsNullOrEmpty(emailClaim))
                {
                    throw new Exception("Email claim not found in token");
                }
                
                // 3. Get user from our database
                var user = await _dbContext.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Username == emailClaim);
                
                if (user == null)
                {
                    throw new Exception("User not found in application database");
                }
                
                // 4. Generate new application JWT with roles
                var appToken = GenerateApplicationJwt(user);
                
                // Return result
                return new AuthResult
                {
                    Success = true,
                    Token = appToken,
                    RefreshToken = refreshResult.RefreshToken,
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Role = user.Role.ToString(),
                        DepartmentId = user.DepartmentId,
                        DepartmentName = user.Department?.Name
                    }
                };
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.Error.WriteLine($"Token refresh error: {ex.Message}");
                
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Token refresh failed: " + ex.Message
                };
            }
        }
        
        /// <summary>
        /// Sends a password reset email via Supabase
        /// </summary>
        public async Task<AuthResult> ResetPasswordAsync(string email)
        {
            try
            {
                // Check if user exists in our database
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Username == email);
                
                if (user == null)
                {
                    // Don't reveal if user exists for security reasons
                    return new AuthResult
                    {
                        Success = true,
                        Message = "If your email is registered, you will receive a password reset link."
                    };
                }
                
                // Send password reset request to Supabase
                var resetRequest = new 
                {
                    email
                };
                
                var resetResponse = await _httpClient.PostAsJsonAsync(
                    $"{_supabaseUrl}/auth/v1/recover",
                    resetRequest);
                
                if (!resetResponse.IsSuccessStatusCode)
                {
                    var errorContent = await resetResponse.Content.ReadAsStringAsync();
                    Console.Error.WriteLine($"Password reset request failed: {errorContent}");
                    
                    // Don't reveal specific error to user
                    return new AuthResult
                    {
                        Success = true,
                        Message = "If your email is registered, you will receive a password reset link."
                    };
                }
                
                return new AuthResult
                {
                    Success = true,
                    Message = "Password reset email sent. Please check your inbox."
                };
            }
            catch (Exception ex)
            {
                // Log exception but don't reveal details to user
                Console.Error.WriteLine($"Password reset error: {ex.Message}");
                
                return new AuthResult
                {
                    Success = true, // Return true for security (don't reveal errors)
                    Message = "If your email is registered, you will receive a password reset link."
                };
            }
        }
        
        /// <summary>
        /// Logs out the user by invalidating tokens (where possible)
        /// </summary>
        public async Task<AuthResult> LogoutAsync(string token)
        {
            try
            {
                // Note: JWT tokens can't be truly invalidated without a blacklist
                // For Supabase, we'd ideally call their logout endpoint
                
                // Extract Supabase session token if available
                var supabaseSessionToken = ExtractSupabaseSession(token);
                
                if (!string.IsNullOrEmpty(supabaseSessionToken))
                {
                    // Call Supabase logout endpoint
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseSessionToken}");
                    await _httpClient.PostAsync($"{_supabaseUrl}/auth/v1/logout", null);
                }
                
                return new AuthResult
                {
                    Success = true,
                    Message = "Logged out successfully"
                };
            }
            catch (Exception ex)
            {
                // Log exception
                Console.Error.WriteLine($"Logout error: {ex.Message}");
                
                // Still return success since client will clear tokens anyway
                return new AuthResult
                {
                    Success = true,
                    Message = "Logged out successfully"
                };
            }
        }
        
        /// <summary>
        /// Validates a JWT token for API calls
        /// </summary>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSecret);
                
                // Set validation parameters
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
                
                // Validate token and return principal
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Token validation error: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Handles login for test admin account in development
        /// </summary>
        private async Task<AuthResult> HandleTestAdminLoginAsync()
        {
            // Find or create admin user
            var adminUser = await _dbContext.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Username == "admin@test.com");
                
            if (adminUser == null)
            {
                // Get or create admin department
                var adminDept = await _dbContext.Departments
                    .FirstOrDefaultAsync(d => d.Name == "Administration");
                    
                if (adminDept == null)
                {
                    adminDept = new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = "Administration"
                    };
                    _dbContext.Departments.Add(adminDept);
                    await _dbContext.SaveChangesAsync();
                }
                
                // Create admin user
                adminUser = new User
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    Username = "admin@test.com",
                    Role = RoleType.Admin,
                    DepartmentId = adminDept.Id,
                    SupabaseId = "test-admin-id"
                };
                _dbContext.Users.Add(adminUser);
                await _dbContext.SaveChangesAsync();
            }
            
            // Generate token for admin
            var token = GenerateApplicationJwt(adminUser);
            
            return new AuthResult
            {
                Success = true,
                Token = token,
                RefreshToken = "temp-refresh-token-for-test-admin",
                User = new UserInfo
                {
                    Id = adminUser.Id,
                    Username = adminUser.Username,
                    Role = adminUser.Role.ToString(),
                    DepartmentId = adminUser.DepartmentId,
                    DepartmentName = adminUser.Department?.Name
                }
            };
        }
        
        /// <summary>
        /// Generates a JWT token containing user information and roles
        /// </summary>
        private string GenerateApplicationJwt(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            
            var claims = new List<Claim>
            { 
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };
            
            if (user.DepartmentId.HasValue)
            {
                claims.Add(new Claim("DepartmentId", user.DepartmentId.Value.ToString()));
            }
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8), // 8 hour token validity
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
        /// <summary>
        /// Attempts to extract Supabase session token from an application token
        /// </summary>
        private string? ExtractSupabaseSession(string token)
        {
            try
            {
                // In a real implementation, you would store the Supabase session token
                // alongside your application token or in a user session
                // For this example, we'll just return null
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
    
    // Response models
    
    /// <summary>
    /// Result returned from authentication operations
    /// </summary>
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public UserInfo? User { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
    }
    
    /// <summary>
    /// User information returned after authentication
    /// </summary>
    public class UserInfo
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
    }
    
    /// <summary>
    /// Request model for user signup
    /// </summary>
    public class SignupRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public bool TermsAgreed { get; set; }
    }
    
    /// <summary>
    /// Supabase authentication response
    /// </summary>
    public class SupabaseAuthResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonProperty("user")]
        public SupabaseUser? User { get; set; }
    }

    /// <summary>
    /// Supabase user information
    /// </summary>
    public class SupabaseUser
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;
    }
}
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Json;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        private readonly AuditService _auditService;
        private readonly HttpClient _httpClient;
        private readonly string _supabaseUrl;
        private readonly string _supabaseKey;

        public AuthController(
            AppDbContext context, 
            IConfiguration configuration, 
            INotificationService notificationService,
            AuditService auditService,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _notificationService = notificationService;
            _auditService = auditService;
            _httpClient = httpClientFactory.CreateClient();
            
            // Get Supabase configuration
            _supabaseUrl = $"https://{_configuration["Supabase:ProjectRef"]}.supabase.co";
            _supabaseKey = _configuration["Supabase:AnonKey"];
            
            // Set default headers for Supabase API calls
            _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // First, authenticate with Supabase
                var supabaseAuthRequest = new
                {
                    email = request.Email,
                    password = request.Password
                };

                var authResponse = await _httpClient.PostAsJsonAsync(
                    $"{_supabaseUrl}/auth/v1/token?grant_type=password",
                    supabaseAuthRequest);

                if (!authResponse.IsSuccessStatusCode)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                var authResult = await authResponse.Content.ReadFromJsonAsync<SupabaseAuthResponse>();
                
                // Now, get the user from our application database
                var user = await _context.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Username == request.Email);

                if (user == null)
                {
                    return Unauthorized(new { message = "User not found in application database" });
                }

                // Log the login
                await _auditService.LogActionAsync(
                    "Authentication",
                    user.Id,
                    "Login",
                    "User logged in successfully");

                // Return the Supabase access token and application user info
                return Ok(new { 
                    token = authResult.AccessToken, 
                    refreshToken = authResult.RefreshToken,
                    user = new { 
                        id = user.Id, 
                        username = user.Username, 
                        role = user.Role.ToString() 
                    } 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Authentication error: {ex.Message}" });
            }
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
            try
            {
                // Check if email is already in use in our application
                if (await _context.Users.AnyAsync(u => u.Username == request.Email))
                {
                    return BadRequest(new { message = "Email is already in use" });
                }

                // Create the user in Supabase
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
                    return BadRequest(new { message = $"Failed to create account: {errorContent}" });
                }

                var signupResult = await signupResponse.Content.ReadFromJsonAsync<SupabaseAuthResponse>();

                // Get holding department
                var holdingDepartment = await _context.Departments
                    .FirstOrDefaultAsync(d => d.Name == "Holding");

                if (holdingDepartment == null)
                {
                    // Create holding department if it doesn't exist
                    holdingDepartment = new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = "Holding"
                    };
                    _context.Departments.Add(holdingDepartment);
                    await _context.SaveChangesAsync();
                }

                // Create new user in holding department
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = request.Email,
                    Role = RoleType.ProjectManager, // Default role for new users
                    DepartmentId = holdingDepartment.Id,
                    // Store Supabase UID for reference
                    SupabaseId = signupResult.User.Id
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Send welcome notification
                await _notificationService.NotifyAsync(
                    "Welcome to Enterprise PMO! Your account has been created and is waiting for department assignment.", 
                    user.Username);

                // Also notify admins about new user registration
                var adminUsers = await _context.Users
                    .Where(u => u.Role == RoleType.Admin)
                    .ToListAsync();

                foreach (var admin in adminUsers)
                {
                    await _notificationService.NotifyAsync(
                        $"New user registration: {user.Username}. Please assign to appropriate department.", 
                        admin.Username);
                }

                // Log the registration
                await _auditService.LogActionAsync(
                    "Authentication",
                    user.Id,
                    "Register",
                    "New user registered and placed in holding department");

                return Ok(new { 
                    message = "User registered successfully and placed in holding department",
                    token = signupResult.AccessToken,
                    refreshToken = signupResult.RefreshToken
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Registration error: {ex.Message}" });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Get the current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    // Log the logout
                    await _auditService.LogActionAsync(
                        "Authentication",
                        userId,
                        "Logout",
                        "User logged out");
                }

                // Get the JWT token from the request header
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                
                // Call Supabase to invalidate the token (sign out)
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                var signoutResponse = await _httpClient.PostAsync(
                    $"{_supabaseUrl}/auth/v1/logout", 
                    null);

                if (!signoutResponse.IsSuccessStatusCode)
                {
                    // Log the error but still consider it a successful logout on our end
                    Console.WriteLine("Failed to logout from Supabase: " + await signoutResponse.Content.ReadAsStringAsync());
                }

                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                // Log the error but still return success to the client
                Console.WriteLine("Error during logout: " + ex.Message);
                return Ok(new { message = "Logged out successfully" });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                // Call Supabase to refresh token
                var refreshRequest = new
                {
                    refresh_token = request.RefreshToken
                };

                var refreshResponse = await _httpClient.PostAsJsonAsync(
                    $"{_supabaseUrl}/auth/v1/token?grant_type=refresh_token",
                    refreshRequest);

                if (!refreshResponse.IsSuccessStatusCode)
                {
                    return Unauthorized(new { message = "Invalid or expired refresh token" });
                }

                var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<SupabaseAuthResponse>();

                // Get user info
                var claims = ParseJwtToken(refreshResult.AccessToken);
                var email = claims.FirstOrDefault(c => c.Type == "email")?.Value;

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == email);

                if (user == null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                return Ok(new
                {
                    token = refreshResult.AccessToken,
                    refreshToken = refreshResult.RefreshToken,
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        role = user.Role.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Token refresh error: {ex.Message}" });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                // Call Supabase to send password reset email
                var resetRequest = new
                {
                    email = request.Email
                };

                var resetResponse = await _httpClient.PostAsJsonAsync(
                    $"{_supabaseUrl}/auth/v1/recover",
                    resetRequest);

                if (!resetResponse.IsSuccessStatusCode)
                {
                    return BadRequest(new { message = "Failed to send password reset email" });
                }

                return Ok(new { message = "Password reset instructions sent to your email" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Password reset error: {ex.Message}" });
            }
        }

        // Helper method to parse JWT token
        private IEnumerable<Claim> ParseJwtToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims;
        }
    }

    public class LoginRequest
    {
        [JsonProperty("email")]
        public required string Email { get; set; }

        [JsonProperty("password")]
        public required string Password { get; set; }

        [JsonProperty("rememberMe")]
        public bool RememberMe { get; set; }
    }

    public class SignupRequest
    {
        [JsonProperty("firstName")]
        public required string FirstName { get; set; }

        [JsonProperty("lastName")]
        public required string LastName { get; set; }

        [JsonProperty("email")]
        public required string Email { get; set; }

        [JsonProperty("departmentId")]
        public Guid DepartmentId { get; set; }

        [JsonProperty("password")]
        public required string Password { get; set; }

        [JsonProperty("confirmPassword")]
        public required string ConfirmPassword { get; set; }

        [JsonProperty("termsAgreed")]
        public bool TermsAgreed { get; set; }
    }

    public class RefreshTokenRequest
    {
        [JsonProperty("refreshToken")]
        public required string RefreshToken { get; set; }
    }

    public class ResetPasswordRequest
    {
        [JsonProperty("email")]
        public required string Email { get; set; }
    }

    public class SupabaseAuthResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("user")]
        public SupabaseUser User { get; set; }
    }

    public class SupabaseUser
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
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
            var projectRef = configuration["Supabase:ProjectRef"];
            _supabaseUrl = !string.IsNullOrEmpty(projectRef) 
                ? $"https://{projectRef}.supabase.co" 
                : "https://example.supabase.co";
                
            _supabaseKey = configuration["Supabase:AnonKey"] ?? "default-key-for-dev";
            
            // Set default headers for Supabase API calls
            _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Debug logging
            Console.WriteLine($"Login attempt for email: {request.Email}");
            
            try
            {
                // TEMPORARY TEST LOGIN - FOR DEVELOPMENT ONLY
                // This allows you to login with admin@test.com / Password123!
                if (request.Email == "admin@test.com" && request.Password == "Password123!")
                {
                    Console.WriteLine("Using test account login");
                    
                    // Find or create an admin user
                    var adminUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Username == "admin@test.com");
                        
                    if (adminUser == null)
                    {
                        // Get or create admin department
                        var adminDept = await _context.Departments
                            .FirstOrDefaultAsync(d => d.Name == "Administration");
                            
                        if (adminDept == null)
                        {
                            adminDept = new Department
                            {
                                Id = Guid.NewGuid(),
                                Name = "Administration"
                            };
                            _context.Departments.Add(adminDept);
                            await _context.SaveChangesAsync();
                        }
                        
                        // Create admin user
                        adminUser = new User
                        {
                            Id = Guid.NewGuid(),
                            Username = "admin@test.com",
                            Role = RoleType.Admin,
                            DepartmentId = adminDept.Id,
                            SupabaseId = "test-admin-id"
                        };
                        _context.Users.Add(adminUser);
                        await _context.SaveChangesAsync();
                        
                        Console.WriteLine($"Created test admin user with ID: {adminUser.Id}");
                    }
                    
                    // Create a test token
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "your-temporary-secret-key-for-testing-only");
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new[] 
                        { 
                            new Claim(ClaimTypes.Name, adminUser.Username),
                            new Claim(ClaimTypes.NameIdentifier, adminUser.Id.ToString()),
                            new Claim(ClaimTypes.Role, adminUser.Role.ToString())
                        }),
                        Expires = DateTime.UtcNow.AddDays(1),
                        SigningCredentials = new SigningCredentials(
                            new SymmetricSecurityKey(key), 
                            SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    
                    // Log the login in the audit trail
                    await _auditService.LogActionAsync(
                        "Authentication",
                        adminUser.Id,
                        "Login",
                        "Test admin user logged in successfully");
                    
                    return Ok(new { 
                        token = tokenHandler.WriteToken(token), 
                        refreshToken = "temp-refresh-token-for-test-admin",
                        user = new { 
                            id = adminUser.Id, 
                            username = adminUser.Username, 
                            role = adminUser.Role.ToString() 
                        } 
                    });
                }

                // Standard Supabase authentication
                try {
                    // First, authenticate with Supabase
                    var supabaseAuthRequest = new
                    {
                        email = request.Email,
                        password = request.Password
                    };

                    Console.WriteLine($"Attempting Supabase auth at URL: {_supabaseUrl}/auth/v1/token?grant_type=password");
                    
                    var authResponse = await _httpClient.PostAsJsonAsync(
                        $"{_supabaseUrl}/auth/v1/token?grant_type=password",
                        supabaseAuthRequest);

                    if (!authResponse.IsSuccessStatusCode)
                    {
                        var errorContent = await authResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"Supabase auth failed: {errorContent}");
                        return Unauthorized(new { message = "Invalid email or password" });
                    }

                    var authResult = await authResponse.Content.ReadFromJsonAsync<SupabaseAuthResponse>();
                    
                    // Now, get the user from our application database
                    var user = await _context.Users
                        .Include(u => u.Department)
                        .FirstOrDefaultAsync(u => u.Username == request.Email);

                    if (user == null)
                    {
                        Console.WriteLine("User not found in application database");
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
                        token = authResult?.AccessToken, 
                        refreshToken = authResult?.RefreshToken,
                        user = new { 
                            id = user.Id, 
                            username = user.Username, 
                            role = user.Role.ToString() 
                        } 
                    });
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error during Supabase authentication: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = $"Authentication error: {ex.Message}" });
            }
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
            // Validate request first
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest(new { message = "Email is required" });
                
            if (string.IsNullOrEmpty(request.Password))
                return BadRequest(new { message = "Password is required" });
                
            if (request.Password != request.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match" });
                
            if (!request.TermsAgreed)
                return BadRequest(new { message = "You must agree to the terms" });
                
            try
            {
                Console.WriteLine($"Signup attempt for email: {request.Email}");
                
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

                Console.WriteLine($"Calling Supabase signup API at: {_supabaseUrl}/auth/v1/signup");
                
                try {
                    var signupResponse = await _httpClient.PostAsJsonAsync(
                        $"{_supabaseUrl}/auth/v1/signup",
                        supabaseSignupRequest);

                    if (!signupResponse.IsSuccessStatusCode)
                    {
                        var errorContent = await signupResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"Supabase signup failed: {errorContent}");
                        return BadRequest(new { message = $"Failed to create account: {errorContent}" });
                    }

                    var signupResult = await signupResponse.Content.ReadFromJsonAsync<SupabaseAuthResponse>();
                    if (signupResult == null)
                    {
                        Console.WriteLine("Null response from Supabase");
                        return BadRequest(new { message = "Invalid response from authentication service" });
                    }
                    
                    // Handle Supabase signup success
                    Console.WriteLine("Supabase signup successful, creating local user");
                    
                    // Get holding department
                    var departmentId = request.DepartmentId;
                    if (departmentId == Guid.Empty)
                    {
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
                            Console.WriteLine($"Created Holding department with ID: {holdingDepartment.Id}");
                        }
                        
                        departmentId = holdingDepartment.Id;
                    }

                    // Create new user in database
                    var user = new User
                    {
                        Id = Guid.NewGuid(),
                        Username = request.Email,
                        Role = RoleType.ProjectManager, // Default role for new users
                        DepartmentId = departmentId,
                        // Store Supabase UID for reference
                        SupabaseId = signupResult.User?.Id
                    };

                    Console.WriteLine($"Creating new user with ID: {user.Id}, Department: {departmentId}");
                    
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    Console.WriteLine("User created successfully, sending notifications");
                    
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
                catch (Exception ex) {
                    Console.WriteLine($"Error during Supabase signup: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
                // Skip if it's a test token
                if (token != "temp-refresh-token-for-test-admin")
                {
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    var signoutResponse = await _httpClient.PostAsync(
                        $"{_supabaseUrl}/auth/v1/logout", 
                        null);

                    if (!signoutResponse.IsSuccessStatusCode)
                    {
                        // Log the error but still consider it a successful logout on our end
                        Console.WriteLine("Failed to logout from Supabase: " + await signoutResponse.Content.ReadAsStringAsync());
                    }
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
                // Skip Supabase for test token
                if (request.RefreshToken == "temp-refresh-token-for-test-admin")
                {
                    var adminUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Username == "admin@test.com");
                        
                    if (adminUser == null)
                    {
                        return Unauthorized(new { message = "Test admin user not found" });
                    }
                    
                    // Create a new test token
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "your-temporary-secret-key-for-testing-only");
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new[] 
                        { 
                            new Claim(ClaimTypes.Name, adminUser.Username),
                            new Claim(ClaimTypes.NameIdentifier, adminUser.Id.ToString()),
                            new Claim(ClaimTypes.Role, adminUser.Role.ToString())
                        }),
                        Expires = DateTime.UtcNow.AddDays(1),
                        SigningCredentials = new SigningCredentials(
                            new SymmetricSecurityKey(key), 
                            SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    
                    return Ok(new
                    {
                        token = tokenHandler.WriteToken(token),
                        refreshToken = "temp-refresh-token-for-test-admin",
                        user = new
                        {
                            id = adminUser.Id,
                            username = adminUser.Username,
                            role = adminUser.Role.ToString()
                        }
                    });
                }
                
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
                    var errorContent = await resetResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Password reset request failed: {errorContent}");
                    return BadRequest(new { message = "Failed to send password reset email" });
                }

                return Ok(new { message = "Password reset instructions sent to your email" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Password reset error: {ex.Message}");
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

    // Request models for the API
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class SignupRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public bool TermsAgreed { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

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

    public class SupabaseUser
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;
    }
}
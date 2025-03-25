using System;
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

        public AuthController(
            AppDbContext context, 
            IConfiguration configuration, 
            INotificationService notificationService,
            AuditService auditService)
        {
            _context = context;
            _configuration = configuration;
            _notificationService = notificationService;
            _auditService = auditService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // In a real implementation, you would check against hashed passwords
            // This is a simplified version for the demo
            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Username == request.Email);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            // Log the login
            await _auditService.LogActionAsync(
                "Authentication",
                user.Id,
                "Login",
                "User logged in successfully");

            return Ok(new { token, user = new { id = user.Id, username = user.Username, role = user.Role.ToString() } });
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
            // Check if email is already in use
            if (await _context.Users.AnyAsync(u => u.Username == request.Email))
            {
                return BadRequest(new { message = "Email is already in use" });
            }

            // Create new user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Email,
                Role = RoleType.ProjectManager, // Default role for new users
                DepartmentId = request.DepartmentId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Send welcome notification
            await _notificationService.NotifyAsync("Welcome to Enterprise PMO!", user.Username);

            // Log the registration
            await _auditService.LogActionAsync(
                "Authentication",
                user.Id,
                "Register",
                "New user registered");

            return Ok(new { message = "User registered successfully" });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
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

            // In a real implementation, you would invalidate the token
            // This is a simplified version for the demo
            return Ok(new { message = "Logged out successfully" });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSecret = _configuration["Supabase:JwtSecret"];
            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException("JWT secret is not configured");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("DepartmentId", user.DepartmentId?.ToString() ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                issuer: $"https://{_configuration["Supabase:ProjectRef"]}.supabase.co",
                audience: _configuration["Supabase:ProjectRef"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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
}
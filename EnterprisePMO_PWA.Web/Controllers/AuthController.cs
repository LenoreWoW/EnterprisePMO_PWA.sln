using System;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly AuditService _auditService;

        public AuthController(
            IAuthService authService,
            AuditService auditService)
        {
            _authService = authService;
            _auditService = auditService;
        }

        /// <summary>
        /// Test endpoint to verify authentication status
        /// </summary>
        [Authorize]
        [HttpGet("test-auth")]
        public IActionResult TestAuth()
        {
            return Ok(new { 
                authenticated = true, 
                username = User.Identity?.Name,
                claims = User.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList()
            });
        }

        /// <summary>
        /// Login endpoint
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

            var result = await _authService.LoginAsync(request.Email, request.Password);
            
            if (!result.Success)
            {
                return Unauthorized(new { message = result.ErrorMessage ?? "Invalid credentials" });
            }
            
            // Return auth data
            return Ok(new { 
                token = result.Token,
                refreshToken = result.RefreshToken,
                user = result.User,
                expiresIn = result.ExpiresIn
            });
        }
        
        /// <summary>
        /// User registration endpoint
        /// </summary>
        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email and password are required" });
            }
            
            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest(new { message = "Passwords do not match" });
            }
            
            if (!request.TermsAgreed)
            {
                return BadRequest(new { message = "You must agree to the terms and conditions" });
            }

            var result = await _authService.SignupAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage ?? "Failed to create account" });
            }
            
            // Return auth data or success message
            return Ok(new { 
                message = result.Message ?? "Account created successfully",
                token = result.Token,
                refreshToken = result.RefreshToken,
                user = result.User,
                expiresIn = result.ExpiresIn
            });
        }
        
        /// <summary>
        /// Logout endpoint
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Get token from authorization header
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            
            var result = await _authService.LogoutAsync(token);
            
            // Log the logout action
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await _auditService.LogActionAsync(
                    "Authentication",
                    Guid.Parse(userId),
                    "Logout",
                    "User logged out successfully"
                );
            }
            
            return Ok(new { message = "Logged out successfully" });
        }
        
        /// <summary>
        /// Token refresh endpoint
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required" });
            }

            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            
            if (!result.Success)
            {
                return Unauthorized(new { message = result.ErrorMessage ?? "Invalid refresh token" });
            }
            
            // Return new auth data
            return Ok(new { 
                token = result.Token,
                refreshToken = result.RefreshToken,
                user = result.User,
                expiresIn = result.ExpiresIn
            });
        }
        
        /// <summary>
        /// Password reset request endpoint
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            var result = await _authService.ResetPasswordAsync(request.Email);
            
            // Always return success to prevent email enumeration attacks
            return Ok(new { 
                message = result.Message ?? "If your email is registered, you will receive a password reset link."
            });
        }

        /// <summary>
        /// Sync user with Supabase endpoint
        /// </summary>
        [Authorize]
        [HttpPost("sync-user")]
        public async Task<IActionResult> SyncUser()
        {
            var email = User.Identity?.Name;
            var supabaseId = User.FindFirst("sub")?.Value;
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(supabaseId))
            {
                return BadRequest(new { message = "Unable to identify user" });
            }
            
            var user = await _authService.SyncUserWithSupabaseAsync(email, supabaseId);
            
            return Ok(new { 
                message = "User synchronized successfully",
                user = user
            });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; }
    }
}
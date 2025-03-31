// EnterprisePMO_PWA.Web/Controllers/AuthController.cs

using System;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly SupabaseAuthService _authService;
        private readonly AuditService _auditService;

        public AuthController(
            SupabaseAuthService authService,
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
        /// Direct login endpoint for test accounts
        /// </summary>
        [HttpPost("direct-login")]
        public async Task<IActionResult> DirectLogin([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

            // Use the same auth service for consistency
            var result = await _authService.LoginAsync(request.Email, request.Password);
            
            if (!result.Success)
            {
                return Unauthorized(new { message = result.ErrorMessage ?? "Invalid credentials" });
            }
            
            // Return auth data
            return Ok(new { 
                token = result.Token,
                refreshToken = result.RefreshToken,
                user = result.User
            });
        }

        /// <summary>
        /// Standard login endpoint using Supabase
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
            
            // Log successful login
            await LogLoginSuccess(result.User?.Id.ToString() ?? "unknown", result.User?.Username ?? "unknown");
            
            // Return auth data
            return Ok(new { 
                token = result.Token,
                refreshToken = result.RefreshToken,
                user = result.User
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
            
            // Log successful registration
            await _auditService.LogActionAsync(
                "Authentication",
                result.User?.Id ?? Guid.Empty,
                "Registration",
                "New user account created successfully"
            );
            
            // Return auth data (if auto-login enabled) or success message
            return Ok(new { 
                message = result.Message ?? "Account created successfully",
                token = result.Token,
                refreshToken = result.RefreshToken,
                user = result.User
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
                user = result.User
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
        /// Logs a successful login attempt for audit purposes
        /// </summary>
        private async Task LogLoginSuccess(string userId, string username)
        {
            try
            {
                if (Guid.TryParse(userId, out Guid userGuid))
                {
                    await _auditService.LogActionAsync(
                        "Authentication",
                        userGuid,
                        "Login",
                        "User logged in successfully"
                    );
                }
                else
                {
                    // Fallback if we don't have a valid user ID
                    await _auditService.LogAction(
                        username,
                        "Login",
                        "Authentication",
                        "User logged in successfully"
                    );
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw - authentication should still succeed
                Console.Error.WriteLine($"Error logging authentication: {ex.Message}");
            }
        }
    }
    
    // Request models for the API
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}
// Authentication Consolidation Plan
// EnterprisePMO Portal

/*
CURRENT SITUATION:
==================
The application currently has two separate authentication mechanisms:
1. Custom auth system (auth.js)
2. Supabase authentication (supabaseClient.js)

This creates redundancy, potential security issues, and maintenance overhead.

CONSOLIDATION STRATEGY:
======================
We'll consolidate to use Supabase as the primary authentication provider while
ensuring seamless integration with the application's existing role-based permissions system.
*/

// Step 1: Create a unified AuthService in the application layer
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

    public class AuthService : IAuthService
    {
        private readonly SupabaseClient _supabaseClient;
        private readonly AppDbContext _dbContext;
        private readonly AuditService _auditService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            SupabaseClient supabaseClient, 
            AppDbContext dbContext,
            AuditService auditService,
            ILogger<AuthService> logger)
        {
            _supabaseClient = supabaseClient;
            _dbContext = dbContext;
            _auditService = auditService;
            _logger = logger;
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
                        Name = "Holding",
                        Description = "Default department for new users"
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
                    SupabaseId = response.User.Id,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
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
                // Call Supabase to send reset password email
                var response = await _supabaseClient.Auth.ResetPasswordForEmail(
                    email, 
                    new ResetPasswordForEmailOptions
                    {
                        RedirectTo = $"{_configuration["AppUrl"]}/account/reset-password"
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
                    Name = "Holding",
                    Description = "Default department for new users"
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
                DepartmentId = holdingDepartment.Id,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
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

// Step 2: Update the AuthController to use the unified AuthService
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
}

// Step 3: Create a SupabaseClient service for .NET backend
namespace EnterprisePMO_PWA.Infrastructure.Services
{
    public class SupabaseClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _supabaseUrl;
        private readonly string _supabaseKey;
        private readonly ILogger<SupabaseClient> _logger;
        
        public SupabaseClient(
            IConfiguration configuration,
            HttpClient httpClient,
            ILogger<SupabaseClient> logger)
        {
            _httpClient = httpClient;
            _supabaseUrl = configuration["Supabase:Url"];
            _supabaseKey = configuration["Supabase:Key"];
            _logger = logger;
            
            // Configure HTTP client
            _httpClient.BaseAddress = new Uri(_supabaseUrl);
            _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
        }
        
        // Authentication methods delegated to Auth property
        public SupabaseAuthClient Auth { get; }
        
        // Other Supabase methods as needed
    }
    
    public class SupabaseAuthClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        
        public SupabaseAuthClient(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        
        public async Task<SupabaseAuthResponse> SignInWithPassword(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/auth/v1/token?grant_type=password", new
                {
                    email,
                    password
                });
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<SupabaseAuthSuccessResponse>();
                    return new SupabaseAuthResponse
                    {
                        User = result.User,
                        Session = result.Session
                    };
                }
                
                var error = await response.Content.ReadFromJsonAsync<SupabaseErrorResponse>();
                return new SupabaseAuthResponse
                {
                    Error = error
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing in with password");
                return new SupabaseAuthResponse
                {
                    Error = new SupabaseErrorResponse
                    {
                        Message = "An unexpected error occurred"
                    }
                };
            }
        }
        
        // Other auth methods (SignUp, SignOut, etc.)
    }
    
    public class SupabaseAuthResponse
    {
        public SupabaseUser User { get; set; }
        public SupabaseSession Session { get; set; }
        public SupabaseErrorResponse Error { get; set; }
    }
    
    public class SupabaseUser
    {
        public string Id { get; set; }
        public string Email { get; set; }
        // Other user properties
    }
    
    public class SupabaseSession
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        // Other session properties
    }
    
    public class SupabaseErrorResponse
    {
        public string Message { get; set; }
        // Other error properties
    }
}

// Step 4: Update frontend authentication client to use only Supabase
// Consolidated auth.js file (replace both auth.js and supabaseClient.js)
/*
import { createClient } from '@supabase/supabase-js';

// Supabase configuration
const supabaseUrl = 'https://pffjdvahsgmtybxrhnla.supabase.co';
const supabaseKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InBmZmpkdmFoc2dtdHlieHJobmxhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDI2ODk3NDYsImV4cCI6MjA1ODI2NTc0Nn0.6OPXiW2QwxvA42F1XcN83bHmtdM7NhulvDaqXxIE9hk';

// Initialize the Supabase client
const supabase = createClient(supabaseUrl, supabaseKey);

/**
 * Unified authentication service for EnterprisePMO
 */
class AuthManager {
  /**
   * Get token from localStorage or query string
   * @returns {string|null} Authentication token
   */
  getToken() {
    // First try from localStorage
    let token = localStorage.getItem('auth_token');
    
    // If not in localStorage, check query string
    if (!token) {
      const urlParams = new URLSearchParams(window.location.search);
      token = urlParams.get('auth_token');
      
      // If found in query string, save to localStorage
      if (token) {
        localStorage.setItem('auth_token', token);
        
        // Clean up URL by removing token parameter
        const newUrl = window.location.pathname + 
          window.location.search.replace(/[\?&]auth_token=[^&]+(&|$)/, '$1');
        window.history.replaceState({}, document.title, newUrl);
      }
    }
    
    return token;
  }

  /**
   * Store authentication data in localStorage
   * @param {Object} authData - Authentication data from server
   */
  setTokens(authData) {
    if (authData.token) {
      localStorage.setItem('auth_token', authData.token);
    }
    
    if (authData.refreshToken) {
      localStorage.setItem('refresh_token', authData.refreshToken);
    }
    
    if (authData.user) {
      localStorage.setItem('user_info', JSON.stringify(authData.user));
    }
    
    if (authData.expiresIn) {
      const expiresAt = new Date();
      expiresAt.setSeconds(expiresAt.getSeconds() + authData.expiresIn);
      localStorage.setItem('expires_at', expiresAt.toISOString());
    }
  }

  /**
   * Clear all auth data from localStorage (logout)
   */
  clearTokens() {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('user_info');
    localStorage.removeItem('expires_at');
  }

  /**
   * Get current authenticated user info
   * @returns {Object|null} User info object or null
   */
  getCurrentUser() {
    const userJson = localStorage.getItem('user_info');
    if (!userJson) return null;
    
    try {
      return JSON.parse(userJson);
    } catch (e) {
      console.error('Error parsing user info:', e);
      return null;
    }
  }

  /**
   * Check if user is authenticated
   * @returns {boolean} True if authenticated
   */
  isAuthenticated() {
    const token = this.getToken();
    if (!token) return false;
    
    // Check if token is expired
    const expiresAt = localStorage.getItem('expires_at');
    if (expiresAt) {
      const expiryDate = new Date(expiresAt);
      if (expiryDate < new Date()) {
        return false;
      }
    }
    
    return true;
  }

  /**
   * Login with email and password
   * @param {string} email - User email
   * @param {string} password - User password
   * @returns {Promise<Object>} Login result
   */
  async login(email, password) {
    try {
      // First authenticate with Supabase directly
      const { data: supabaseData, error: supabaseError } = await supabase.auth.signInWithPassword({
        email,
        password
      });
      
      if (supabaseError) throw new Error(supabaseError.message);
      
      // Then call our backend API to sync the authentication
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${supabaseData.session.access_token}`
        },
        body: JSON.stringify({ email, password })
      });
      
      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Login failed');
      }
      
      const data = await response.json();
      
      // Save authentication data
      this.setTokens(data);
      
      // Update UI state
      this.updateAuthUI(true);
      
      return data;
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    }
  }

  /**
   * Submit signup form data
   * @param {Object} formData - User signup data
   * @returns {Promise<Object>} Signup result
   */
  async signup(formData) {
    try {
      // Register with our backend API, which handles Supabase registration
      const response = await fetch('/api/auth/signup', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(formData)
      });
      
      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Signup failed');
      }
      
      const data = await response.json();
      
      // Auto login if token returned
      if (data.token) {
        this.setTokens(data);
        this.updateAuthUI(true);
      }
      
      return data;
    } catch (error) {
      console.error('Signup error:', error);
      throw error;
    }
  }

  /**
   * Logout user
   * @returns {Promise<void>}
   */
  async logout() {
    const token = this.getToken();
    
    try {
      // Sign out from Supabase
      await supabase.auth.signOut();
      
      // Notify backend about logout
      if (token) {
        await fetch('/api/auth/logout', {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        });
      }
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      // Always clear tokens regardless of API success
      this.clearTokens();
      this.updateAuthUI(false);
      
      // Redirect to login
      window.location.href = '/Account/Login';
    }
  }

  /**
   * Refresh authentication token
   * @returns {Promise<Object>} New token data
   */
  async refreshToken() {
    const refreshToken = localStorage.getItem('refresh_token');
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }
    
    try {
      // First refresh token with Supabase
      const { data: supabaseData, error: supabaseError } = await supabase.auth.refreshSession({
        refresh_token: refreshToken
      });
      
      if (supabaseError) throw new Error(supabaseError.message);
      
      // Then sync with our backend
      const response = await fetch('/api/auth/refresh-token', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ refreshToken })
      });
      
      if (!response.ok) {
        throw new Error('Failed to refresh token');
      }
      
      const data = await response.json();
      
      this.setTokens({
        token: data.token,
        refreshToken: data.refreshToken,
        user: data.user,
        expiresIn: data.expiresIn
      });
      
      return data;
    } catch (error) {
      console.error('Token refresh error:', error);
      this.clearTokens();
      throw error;
    }
  }

  /**
   * Request password reset
   * @param {string} email - User email
   * @returns {Promise<Object>} Password reset result
   */
  async resetPassword(email) {
    try {
      const response = await fetch('/api/auth/reset-password', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ email })
      });
      
      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || 'Password reset failed');
      }
      
      const data = await response.json();
      return data;
    } catch (error) {
      console.error('Password reset error:', error);
      throw error;
    }
  }

  /**
   * Set up fetch interceptor to add auth token to requests
   */
  setupFetchInterceptor() {
    const originalFetch = window.fetch;
    
    window.fetch = async (url, options = {}) => {
      // Don't add auth header for login/signup requests
      if (typeof url === 'string' && 
          (url.includes('/api/auth/login') || 
           url.includes('/api/auth/signup') || 
           url.includes('/api/auth/direct-login'))) {
        return originalFetch(url, options);
      }
      
      // Clone options to avoid modification of original
      const newOptions = {...options};
      newOptions.headers = newOptions.headers || {};
      
      // Add auth token if available and not already added
      const token = this.getToken();
      if (token && !newOptions.headers.Authorization && !newOptions.headers.authorization) {
        newOptions.headers.Authorization = `Bearer ${token}`;
      }
      
      // Check if token is about to expire and refresh if needed
      const expiresAt = localStorage.getItem('expires_at');
      if (expiresAt) {
        const expiryDate = new Date(expiresAt);
        const now = new Date();
        
        // If token expires within the next 5 minutes, refresh it
        if (expiryDate <= new Date(now.getTime() + 5 * 60 * 1000)) {
          try {
            await this.refreshToken();
            // Update token in headers after refresh
            const newToken = this.getToken();
            if (newToken) {
              newOptions.headers.Authorization = `Bearer ${newToken}`;
            }
          } catch (refreshError) {
            console.warn('Token refresh failed:', refreshError);
            // Continue with request using existing token
          }
        }
      }
      
      try {
        const response = await originalFetch(url, newOptions);
        
        // Handle 401 Unauthorized
        if (response.status === 401) {
          console.error('401 Unauthorized response received');
          
          // Try to refresh token once
          if (!url.includes('/api/auth/refresh-token')) {
            try {
              await this.refreshToken();
              
              // Retry the original request with new token
              const retryToken = this.getToken();
              if (retryToken) {
                newOptions.headers.Authorization = `Bearer ${retryToken}`;
                return originalFetch(url, newOptions);
              }
            } catch (refreshError) {
              // If refresh fails, clear tokens and redirect to login
              this.clearTokens();
              
              // Redirect to login if not already there
              if (!window.location.pathname.includes('/Account/Login')) {
                window.location.href = '/Account/Login';
              }
            }
          } else {
            // If the refresh token request itself fails with 401, clear tokens
            this.clearTokens();
            
            // Redirect to login if not already there
            if (!window.location.pathname.includes('/Account/Login')) {
              window.location.href = '/Account/Login';
            }
          }
        }
        
        return response;
      } catch (error) {
        console.error('Fetch error:', error);
        throw error;
      }
    };
  }

  /**
   * Apply auth token to links with data-requires-auth attribute
   */
  applyAuthToLinks() {
    const token = this.getToken();
    if (!token) return;
    
    // Add token to links
    document.querySelectorAll('[data-requires-auth="true"]').forEach(link => {
      if (!link.href.includes('auth_token=')) {
        try {
          const url = new URL(link.href);
          url.searchParams.set('auth_token', token);
          link.href = url.toString();
        } catch (e) {
          console.warn('Error updating auth link:', link, e);
        }
      }
    });
  }

  /**
   * Update UI elements based on auth state
   * @param {boolean} isAuth - Whether user is authenticated
   */
  updateAuthUI(isAuth) {
    // Set data attribute on body
    document.body.setAttribute('data-authenticated', isAuth ? 'true' : 'false');
    
    // Show/hide navigation elements
    const navItems = document.querySelectorAll('.nav-items');
    navItems.forEach(el => {
      el.style.display = isAuth ? 'flex' : 'none';
    });
    
    // Show/hide auth-dependent items
    const authItems = document.querySelectorAll('.nav-auth-item');
    authItems.forEach(el => {
      el.style.display = isAuth ? 'block' : 'none';
    });
    
    // Show/hide non-auth items
    const noAuthItems = document.querySelectorAll('.nav-no-auth-item');
    noAuthItems.forEach(el => {
      el.style.display = isAuth ? 'none' : 'flex'; 
    });
    
    // Update username display if authenticated
    if (isAuth) {
      const user = this.getCurrentUser();
      if (user) {
        const usernameDivs = document.querySelectorAll('.current-username');
        usernameDivs.forEach(el => {
          el.textContent = user.username;
        });
        
        // Update admin-only elements visibility
        if (user.role === 'Admin' || user.role === 'MainPMO') {
          document.querySelectorAll('.admin-only').forEach(el => {
            el.style.display = '';
          });
        } else {
          document.querySelectorAll('.admin-only').forEach(el => {
            el.style.display = 'none';
          });
        }
      }
      
      // Apply auth token to links
      this.applyAuthToLinks();
    }
  }

  /**
   * Sets up auth state change listener with Supabase
   */
  setupAuthListener() {
    supabase.auth.onAuthStateChange((event, session) => {
      console.log('Supabase auth state changed:', event);
      
      if (event === 'SIGNED_IN' && session) {
        // We'll let our login/signup handlers handle this case
        // Just ensure we have correct token
        if (!this.getToken() && session.access_token) {
          this.setTokens({
            token: session.access_token,
            refreshToken: session.refresh_token,
            expiresIn: session.expires_in
          });
          
          // Sync with backend
          this.syncWithBackend(session.access_token).catch(console.error);
        }
      } else if (event === 'SIGNED_OUT') {
        // Clear tokens and update UI
        this.clearTokens();
        this.updateAuthUI(false);
      } else if (event === 'TOKEN_REFRESHED' && session) {
        // Update tokens
        this.setTokens({
          token: session.access_token,
          refreshToken: session.refresh_token,
          expiresIn: session.expires_in
        });
      }
    });
  }

  /**
   * Sync user info with backend
   * @param {string} token - Access token
   * @returns {Promise<Object>} Sync result
   */
  async syncWithBackend(token) {
    const response = await fetch('/api/auth/sync-user', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      }
    });
    
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to sync user');
    }
    
    const data = await response.json();
    
    // Update user info
    if (data.user) {
      localStorage.setItem('user_info', JSON.stringify(data.user));
    }
    
    return data;
  }
}

// Create and export the auth service instance
const authManager = new AuthManager();

// Initialize auth on module load
document.addEventListener('DOMContentLoaded', async () => {
  // Setup fetch interceptor
  authManager.setupFetchInterceptor();
  
  // Setup auth listener
  authManager.setupAuthListener();
  
  // Check initial auth state
  const isAuthenticated = authManager.isAuthenticated();
  
  // Update UI based on auth state
  authManager.updateAuthUI(isAuthenticated);
  
  // If authenticated, check if token needs refresh
  if (isAuthenticated) {
    const expiresAt = localStorage.getItem('expires_at');
    if (expiresAt) {
      const expiresAtDate = new Date(expiresAt);
      const now = new Date();
      
      // If token expires within the next hour, refresh it
      if (expiresAtDate <= new Date(now.getTime() + 60 * 60 * 1000)) {
        authManager.refreshToken().catch(error => {
          console.warn('Token refresh failed:', error);
        });
      }
    }
  }
  
  // Set up login form handler
  const loginForm = document.getElementById('loginForm');
  if (loginForm) {
    loginForm.addEventListener('submit', function(e) {
      e.preventDefault();
      
      const emailField = document.getElementById('Email');
      const passwordField = document.getElementById('Password');
      const errorDiv = document.getElementById('loginErrorMessage');
      
      if (!emailField || !passwordField) {
        console.error('Email or password field not found');
        return;
      }
      
      // Show loading state
      if (errorDiv) {
        errorDiv.style.display = 'none';
      }
      
      const submitBtn = loginForm.querySelector('button[type="submit"]');
      if (submitBtn) {
        submitBtn.disabled = true;
        const originalText = submitBtn.innerHTML;
        submitBtn.innerHTML = '<svg class="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg> Signing in...';
      }
      
      const email = emailField.value;
      const password = passwordField.value;
      
      authManager.login(email, password)
        .then(() => {
          // Redirect to dashboard
          window.location.href = '/Dashboard';
        })
        .catch(error => {
          // Show error
          if (errorDiv) {
            errorDiv.textContent = error.message || 'Invalid email or password';
            errorDiv.style.display = 'block';
          }
          
          // Reset submit button
          if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.innerHTML = 'Sign in';
          }
        });
    });
  }
  
  // Set up logout handler
  document.querySelectorAll('form#logoutForm, [data-action="logout"]').forEach(element => {
    element.addEventListener(element.tagName === 'FORM' ? 'submit' : 'click', function(e) {
      e.preventDefault();
      authManager.logout();
    });
  });
});

// Export the auth API
window.authManager = authManager;
*/
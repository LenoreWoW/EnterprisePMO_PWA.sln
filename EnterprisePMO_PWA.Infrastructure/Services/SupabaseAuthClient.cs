using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EnterprisePMO_PWA.Infrastructure.Services
{
    /// <summary>
    /// Client for Supabase authentication operations
    /// </summary>
    public class SupabaseAuthClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        
        public SupabaseAuthClient(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Sign in with email and password
        /// </summary>
        public async Task<SupabaseAuthResponse> SignInWithPassword(string email, string password)
        {
            try
            {
                var requestData = new
                {
                    email,
                    password
                };
                
                var response = await _httpClient.PostAsJsonAsync(
                    "/auth/v1/token?grant_type=password", 
                    requestData,
                    _jsonOptions);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<SupabaseAuthSuccessResponse>(_jsonOptions);
                    return new SupabaseAuthResponse
                    {
                        User = result?.User,
                        Session = result?.Session
                    };
                }
                
                var error = await response.Content.ReadFromJsonAsync<SupabaseErrorResponse>(_jsonOptions);
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
                        Message = "An unexpected error occurred during sign in"
                    }
                };
            }
        }
        
        /// <summary>
        /// Sign up with email and password
        /// </summary>
        public async Task<SupabaseAuthResponse> SignUp(
            string email, 
            string password, 
            SignUpOptions? options = null)
        {
            try
            {
                var requestData = new
                {
                    email,
                    password,
                    data = options?.Data
                };
                
                var response = await _httpClient.PostAsJsonAsync(
                    "/auth/v1/signup", 
                    requestData,
                    _jsonOptions);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<SupabaseAuthSuccessResponse>(_jsonOptions);
                    return new SupabaseAuthResponse
                    {
                        User = result?.User,
                        Session = result?.Session
                    };
                }
                
                var error = await response.Content.ReadFromJsonAsync<SupabaseErrorResponse>(_jsonOptions);
                return new SupabaseAuthResponse
                {
                    Error = error
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing up");
                return new SupabaseAuthResponse
                {
                    Error = new SupabaseErrorResponse
                    {
                        Message = "An unexpected error occurred during sign up"
                    }
                };
            }
        }
        
        /// <summary>
        /// Sign out the current user
        /// </summary>
        public async Task<SupabaseAuthResponse> SignOut(string? token = null)
        {
            try
            {
                // Configure request
                var request = new HttpRequestMessage(HttpMethod.Post, "/auth/v1/logout");
                
                // Add Bearer token if provided
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    return new SupabaseAuthResponse
                    {
                        Success = true
                    };
                }
                
                var error = await response.Content.ReadFromJsonAsync<SupabaseErrorResponse>(_jsonOptions);
                return new SupabaseAuthResponse
                {
                    Error = error
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing out");
                return new SupabaseAuthResponse
                {
                    Error = new SupabaseErrorResponse
                    {
                        Message = "An unexpected error occurred during sign out"
                    }
                };
            }
        }
        
        /// <summary>
        /// Refresh the session using a refresh token
        /// </summary>
        public async Task<SupabaseAuthResponse> RefreshSession(string refreshToken)
        {
            try
            {
                var requestData = new
                {
                    refresh_token = refreshToken
                };
                
                var response = await _httpClient.PostAsJsonAsync(
                    "/auth/v1/token?grant_type=refresh_token", 
                    requestData,
                    _jsonOptions);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<SupabaseAuthSuccessResponse>(_jsonOptions);
                    return new SupabaseAuthResponse
                    {
                        User = result?.User,
                        Session = result?.Session
                    };
                }
                
                var error = await response.Content.ReadFromJsonAsync<SupabaseErrorResponse>(_jsonOptions);
                return new SupabaseAuthResponse
                {
                    Error = error
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing session");
                return new SupabaseAuthResponse
                {
                    Error = new SupabaseErrorResponse
                    {
                        Message = "An unexpected error occurred during session refresh"
                    }
                };
            }
        }
        
        /// <summary>
        /// Send a password reset email
        /// </summary>
        public async Task<SupabaseAuthResponse> ResetPasswordForEmail(
            string email, 
            ResetPasswordForEmailOptions? options = null)
        {
            try
            {
                var requestData = new
                {
                    email,
                    redirect_to = options?.RedirectTo
                };
                
                var response = await _httpClient.PostAsJsonAsync(
                    "/auth/v1/recover", 
                    requestData,
                    _jsonOptions);
                
                if (response.IsSuccessStatusCode)
                {
                    return new SupabaseAuthResponse
                    {
                        Success = true
                    };
                }
                
                var error = await response.Content.ReadFromJsonAsync<SupabaseErrorResponse>(_jsonOptions);
                return new SupabaseAuthResponse
                {
                    Error = error
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email");
                return new SupabaseAuthResponse
                {
                    Error = new SupabaseErrorResponse
                    {
                        Message = "An unexpected error occurred during password reset"
                    }
                };
            }
        }
        
        /// <summary>
        /// Get user data from a JWT token
        /// </summary>
        public async Task<SupabaseUserResponse> GetUser(string token)
        {
            try
            {
                // Configure request
                var request = new HttpRequestMessage(HttpMethod.Get, "/auth/v1/user");
                
                // Add Bearer token
                request.Headers.Add("Authorization", $"Bearer {token}");
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<SupabaseUser>(_jsonOptions);
                    return new SupabaseUserResponse
                    {
                        User = user
                    };
                }
                
                var error = await response.Content.ReadFromJsonAsync<SupabaseErrorResponse>(_jsonOptions);
                return new SupabaseUserResponse
                {
                    Error = error
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user");
                return new SupabaseUserResponse
                {
                    Error = new SupabaseErrorResponse
                    {
                        Message = "An unexpected error occurred while getting user data"
                    }
                };
            }
        }
    }
    
    #region Models
    
    /// <summary>
    /// Supabase authentication response
    /// </summary>
    public class SupabaseAuthResponse
    {
        public bool Success { get; set; }
        public SupabaseUser? User { get; set; }
        public SupabaseSession? Session { get; set; }
        public SupabaseErrorResponse? Error { get; set; }
    }
    
    /// <summary>
    /// Supabase user response
    /// </summary>
    public class SupabaseUserResponse
    {
        public SupabaseUser? User { get; set; }
        public SupabaseErrorResponse? Error { get; set; }
    }
    
    /// <summary>
    /// Supabase successful authentication response
    /// </summary>
    public class SupabaseAuthSuccessResponse
    {
        public SupabaseUser? User { get; set; }
        public SupabaseSession? Session { get; set; }
    }
    
    /// <summary>
    /// Supabase user model
    /// </summary>
    public class SupabaseUser
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        [JsonPropertyName("user_metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
        
        [JsonPropertyName("app_metadata")]
        public Dictionary<string, object>? AppMetadata { get; set; }
        
        public string Role { get; set; } = string.Empty;
        
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }
        
        public bool EmailConfirmed { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? LastSignInAt { get; set; }
    }
    
    /// <summary>
    /// Supabase session model
    /// </summary>
    public class SupabaseSession
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
        
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;
        
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "bearer";
    }
    
    /// <summary>
    /// Supabase error response
    /// </summary>
    public class SupabaseErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string? Code { get; set; }
    }
    
    /// <summary>
    /// Options for sign up
    /// </summary>
    public class SignUpOptions
    {
        public Dictionary<string, object>? Data { get; set; }
    }
    
    /// <summary>
    /// Options for password reset email
    /// </summary>
    public class ResetPasswordForEmailOptions
    {
        public string? RedirectTo { get; set; }
    }
    
    #endregion
}
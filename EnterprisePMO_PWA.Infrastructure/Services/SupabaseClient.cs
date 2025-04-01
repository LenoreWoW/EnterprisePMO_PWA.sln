using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
            
            // Initialize auth client
            Auth = new SupabaseAuthClient(httpClient, logger);
        }
        
        // Authentication methods delegated to Auth property
        public SupabaseAuthClient Auth { get; }
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
        
        public async Task<SupabaseAuthResponse> SignUp(string email, string password, SignUpOptions options = null)
        {
            try
            {
                var payload = new
                {
                    email,
                    password,
                    data = options?.Data
                };
                
                var response = await _httpClient.PostAsJsonAsync("/auth/v1/signup", payload);
                
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
                _logger.LogError(ex, "Error signing up");
                return new SupabaseAuthResponse
                {
                    Error = new SupabaseErrorResponse
                    {
                        Message = "An unexpected error occurred"
                    }
                };
            }
        }
        
        public async Task<SupabaseAuthResponse> RefreshSession(string refreshToken)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/auth/v1/token?grant_type=refresh_token", new
                {
                    refresh_token = refreshToken
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
                _logger.LogError(ex, "Error refreshing session");
                return new SupabaseAuthResponse
                {
                    Error = new SupabaseErrorResponse
                    {
                        Message = "An unexpected error occurred"
                    }
                };
            }
        }
        
        public async Task<SupabaseAuthResponse> ResetPasswordForEmail(string email, ResetPasswordForEmailOptions options = null)
        {
            try
            {
                var payload = new
                {
                    email,
                    redirect_to = options?.RedirectTo
                };
                
                var response = await _httpClient.PostAsJsonAsync("/auth/v1/recover", payload);
                
                if (response.IsSuccessStatusCode)
                {
                    return new SupabaseAuthResponse();
                }
                
                var error = await response.Content.ReadFromJsonAsync<SupabaseErrorResponse>();
                return new SupabaseAuthResponse
                {
                    Error = error
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return new SupabaseAuthResponse
                {
                    Error = new SupabaseErrorResponse
                    {
                        Message = "An unexpected error occurred"
                    }
                };
            }
        }
        
        public async Task<SupabaseAuthResponse> GetUser(string accessToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                
                var response = await _httpClient.GetAsync("/auth/v1/user");
                
                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<SupabaseUser>();
                    return new SupabaseAuthResponse
                    {
                        User = user
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
                _logger.LogError(ex, "Error getting user");
                return new SupabaseAuthResponse
                {
                    Error = new SupabaseErrorResponse
                    {
                        Message = "An unexpected error occurred"
                    }
                };
            }
        }
        
        public async Task<SupabaseAuthResponse> SignOut()
        {
            try
            {
                var response = await _httpClient.PostAsync("/auth/v1/logout", null);
                
                if (response.IsSuccessStatusCode)
                {
                    return new SupabaseAuthResponse();
                }
                
                var error = await response.Content.ReadFromJsonAsync<SupabaseErrorResponse>();
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
                        Message = "An unexpected error occurred"
                    }
                };
            }
        }
    }
    
    public class SupabaseAuthSuccessResponse
    {
        public SupabaseUser User { get; set; }
        public SupabaseSession Session { get; set; }
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
        public JsonElement? UserMetadata { get; set; }
        public JsonElement? AppMetadata { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
    
    public class SupabaseSession
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; }
    }
    
    public class SupabaseErrorResponse
    {
        public string Message { get; set; }
        public string Code { get; set; }
    }
    
    public class SignUpOptions
    {
        public Dictionary<string, object> Data { get; set; }
    }
    
    public class ResetPasswordForEmailOptions
    {
        public string RedirectTo { get; set; }
    }
}
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnterprisePMO_PWA.Infrastructure.Services
{
    /// <summary>
    /// Client for interacting with Supabase API
    /// </summary>
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
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _supabaseUrl = configuration["Supabase:Url"] ?? 
                throw new ArgumentException("Supabase:Url configuration is missing");
            
            _supabaseKey = configuration["Supabase:Key"] ?? 
                throw new ArgumentException("Supabase:Key configuration is missing");
            
            // Configure HTTP client
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_supabaseUrl);
            }
            
            // Ensure apikey header is set
            if (!_httpClient.DefaultRequestHeaders.Contains("apikey"))
            {
                _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
            }
            
            // Initialize auth client
            Auth = new SupabaseAuthClient(_httpClient, _logger);
        }
        
        /// <summary>
        /// Authentication client for Supabase
        /// </summary>
        public SupabaseAuthClient Auth { get; }
        
        /// <summary>
        /// Makes a raw HTTP request to Supabase
        /// </summary>
        public async Task<HttpResponseMessage> RawRequestAsync(
            HttpMethod method, 
            string endpoint, 
            object? data = null, 
            string? token = null)
        {
            var request = new HttpRequestMessage(method, endpoint);
            
            // Add Bearer token if provided
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }
            
            // Add request body if provided
            if (data != null)
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            
            return await _httpClient.SendAsync(request);
        }
    }
}
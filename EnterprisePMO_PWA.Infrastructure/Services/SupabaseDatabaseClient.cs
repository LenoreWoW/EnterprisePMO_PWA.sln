using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EnterprisePMO_PWA.Infrastructure.Services
{
    /// <summary>
    /// Client for Supabase database operations
    /// </summary>
    public class SupabaseDatabaseClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        
        public SupabaseDatabaseClient(
            HttpClient httpClient,
            ILogger logger,
            JsonSerializerOptions jsonOptions)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
        }
        
        /// <summary>
        /// Select records from a table
        /// </summary>
        public async Task<T?> SelectAsync<T>(string table, string? token = null, string? query = null)
        {
            try
            {
                var endpoint = $"/rest/v1/{table}";
                if (!string.IsNullOrEmpty(query))
                {
                    endpoint += $"?{query}";
                }
                
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }
                
                request.Headers.Add("Prefer", "return=representation");
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
                }
                
                _logger.LogError("Error selecting from {Table}: {StatusCode}", table, response.StatusCode);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting from {Table}", table);
                return default;
            }
        }
        
        /// <summary>
        /// Insert a record into a table
        /// </summary>
        public async Task<T?> InsertAsync<T>(string table, object data, string? token = null)
        {
            try
            {
                var endpoint = $"/rest/v1/{table}";
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }
                
                request.Headers.Add("Prefer", "return=representation");
                
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
                }
                
                _logger.LogError("Error inserting into {Table}: {StatusCode}", table, response.StatusCode);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting into {Table}", table);
                return default;
            }
        }
        
        /// <summary>
        /// Update records in a table
        /// </summary>
        public async Task<T?> UpdateAsync<T>(string table, object data, string query, string? token = null)
        {
            try
            {
                var endpoint = $"/rest/v1/{table}?{query}";
                var request = new HttpRequestMessage(HttpMethod.Patch, endpoint);
                
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }
                
                request.Headers.Add("Prefer", "return=representation");
                
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
                }
                
                _logger.LogError("Error updating {Table}: {StatusCode}", table, response.StatusCode);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {Table}", table);
                return default;
            }
        }
        
        /// <summary>
        /// Delete records from a table
        /// </summary>
        public async Task<bool> DeleteAsync(string table, string query, string? token = null)
        {
            try
            {
                var endpoint = $"/rest/v1/{table}?{query}";
                var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
                
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                
                _logger.LogError("Error deleting from {Table}: {StatusCode}", table, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting from {Table}", table);
                return false;
            }
        }
        
        /// <summary>
        /// Execute a stored procedure
        /// </summary>
        public async Task<T?> RpcAsync<T>(string function, object parameters, string? token = null)
        {
            try
            {
                var endpoint = $"/rest/v1/rpc/{function}";
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", $"Bearer {token}");
                }
                
                request.Headers.Add("Prefer", "return=representation");
                
                var json = JsonSerializer.Serialize(parameters, _jsonOptions);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
                }
                
                _logger.LogError("Error executing RPC {Function}: {StatusCode}", function, response.StatusCode);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing RPC {Function}", function);
                return default;
            }
        }
    }
} 
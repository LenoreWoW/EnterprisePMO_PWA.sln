using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnterprisePMO_PWA.Infrastructure.Services
{
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
}
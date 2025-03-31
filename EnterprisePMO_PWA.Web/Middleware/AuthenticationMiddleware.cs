using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EnterprisePMO_PWA.Infrastructure.Data;
using EnterprisePMO_PWA.Application.Services;
using System.Security.Claims;

namespace EnterprisePMO_PWA.Web.Middleware
{
    /// <summary>
    /// Middleware that synchronizes Supabase authentication with our application's user database
    /// </summary>
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext, IAuthService authService)
        {
            // Only process for authenticated requests that have already passed through
            // the authentication middleware and token validation
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Get the claims from the token
                var supabaseIdClaim = context.User.FindFirst("sub");
                var emailClaim = context.User.FindFirst(ClaimTypes.Email) ?? context.User.FindFirst("email");

                if (supabaseIdClaim != null && emailClaim != null)
                {
                    var supabaseId = supabaseIdClaim.Value;
                    var email = emailClaim.Value;

                    // Check if the user exists in our database by Supabase ID
                    var user = await authService.GetUserBySupabaseIdAsync(supabaseId);

                    if (user == null)
                    {
                        // User doesn't exist yet, create it using the sync method
                        user = await authService.SyncUserWithSupabaseAsync(email, supabaseId);
                    }
                    
                    // Ensure the user has the correct username/email (in case it was changed in Supabase)
                    if (user.Username != email)
                    {
                        user.Username = email;
                        await dbContext.SaveChangesAsync();
                    }
                }
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }
}
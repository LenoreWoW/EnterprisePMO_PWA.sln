using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using EnterprisePMO_PWA.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
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

                    // Check if the user exists in our database
                    var user = await dbContext.Users
                        .FirstOrDefaultAsync(u => u.Username == email);

                    if (user != null)
                    {
                        // If the user exists but doesn't have a Supabase ID yet, update it
                        if (string.IsNullOrEmpty(user.SupabaseId))
                        {
                            user.SupabaseId = supabaseId;
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        // If the user is authenticated with Supabase but doesn't exist in our database,
                        // we might want to automatically create a shadow record. This is optional and
                        // depends on whether you want to allow sign-up through Supabase directly.
                        
                        // Get holding department
                        var holdingDepartment = await dbContext.Departments
                            .FirstOrDefaultAsync(d => d.Name == "Holding");

                        if (holdingDepartment != null)
                        {
                            var newUser = new EnterprisePMO_PWA.Domain.Entities.User
                            {
                                Id = Guid.NewGuid(),
                                Username = email,
                                Role = EnterprisePMO_PWA.Domain.Entities.RoleType.ProjectManager, // Default role
                                DepartmentId = holdingDepartment.Id,
                                SupabaseId = supabaseId
                            };

                            dbContext.Users.Add(newUser);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }
}
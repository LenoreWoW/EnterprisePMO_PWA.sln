// Program.cs - Configuration for Authentication Integration

using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Supabase client
builder.Services.AddHttpClient<SupabaseClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Supabase:Url"]);
    client.DefaultRequestHeaders.Add("apikey", builder.Configuration["Supabase:Key"]);
});

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuditService>();

// Configure authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Configure to use Supabase JWT
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // For Supabase JWT validation
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Supabase:Url"],
        ValidAudience = "authenticated",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Supabase:JWT:Secret"]))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Allow JWT token to be provided in query string for certain scenarios (like file downloads)
            var accessToken = context.Request.Query["auth_token"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        
        OnTokenValidated = async context =>
        {
            // Get the user from database using Supabase ID
            var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
            var supabaseId = context.Principal.FindFirstValue("sub");
            
            if (!string.IsNullOrEmpty(supabaseId))
            {
                var user = await authService.GetUserBySupabaseIdAsync(supabaseId);
                
                if (user == null)
                {
                    // User not found in our database, create it
                    var email = context.Principal.FindFirstValue("email");
                    if (!string.IsNullOrEmpty(email))
                    {
                        user = await authService.SyncUserWithSupabaseAsync(email, supabaseId);
                    }
                    else
                    {
                        context.Fail("User not found in application database");
                    }
                }
                
                // Add application-specific claims
                if (user != null)
                {
                    var identity = context.Principal.Identity as ClaimsIdentity;
                    identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                    
                    if (!string.IsNullOrEmpty(user.Username))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Name, user.Username));
                    }
                    
                    if (user.Department != null)
                    {
                        identity.AddClaim(new Claim("Department", user.Department.Id.ToString()));
                        identity.AddClaim(new Claim("DepartmentName", user.Department.Name));
                    }
                }
            }
        }
    };
});

// Add middleware
builder.Services.AddScoped<AuthenticationMiddleware>();

// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Use the authentication middleware
app.UseMiddleware<AuthenticationMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
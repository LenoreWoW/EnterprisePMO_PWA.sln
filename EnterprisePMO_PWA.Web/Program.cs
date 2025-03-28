using EnterprisePMO_PWA.Infrastructure.Data;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Web.Hubs;
using EnterprisePMO_PWA.Web.Services;
using EnterprisePMO_PWA.Web.Authorization;
using EnterprisePMO_PWA.Web.Extensions;
using EnterprisePMO_PWA.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Hangfire;
using Hangfire.MemoryStorage;
using System.Text;

// Setup the web server with in-memory database
var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Keep using the URLs that worked
builder.WebHost.UseUrls("http://0.0.0.0:7000", "https://0.0.0.0:7001");

// Add services to the container
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Add HTTP client factory
builder.Services.AddHttpClient();

// Add in-memory database for development
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Use existing database provider (in-memory or PostgreSQL)
    if (builder.Environment.IsDevelopment())
    {
        options.UseInMemoryDatabase("EnterprisePMO");
    }
    else
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});

// Register application services
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<WeeklyUpdateService>();
builder.Services.AddScoped<ChangeRequestService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<ProjectMemberService>();
builder.Services.AddScoped<WorkflowService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddHttpContextAccessor();

// Register IPermissionService explicitly
builder.Services.AddScoped<EnterprisePMO_PWA.Domain.Services.IPermissionService, EnterprisePMO_PWA.Application.Services.PermissionService>();

// Register new workflow and notification services
builder.Services.AddScoped<ProjectWorkflowService>();

// Add SignalR
builder.Services.AddSignalR();

// Register real-time notification service (Web layer)
builder.Services.AddScoped<IRealTimeNotificationService, SignalRNotificationService>();

// Register notification service (Application layer)
builder.Services.AddScoped<INotificationService, EnhancedNotificationService>();

// Add CORS with more permissive policy for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", corsBuilder =>
    {
        corsBuilder.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
    });
});

// JWT Authentication with simplified configuration for development
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? 
                  Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
                  "your-temporary-secret-key-for-testing-only-make-it-at-least-32-chars";

Console.WriteLine($"JWT Secret Key configured: {(string.IsNullOrEmpty(jwtSecretKey) ? "No" : "Yes")}");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // For development, disable all validations except signing key
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero
    };
    
    // Set auth token from query string for API calls
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Check for token in query string for client-side apps
            var accessToken = context.Request.Query["auth_token"].ToString();
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
                Console.WriteLine($"Using token from query string: {accessToken[..Math.Min(10, accessToken.Length)]}...");
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"Token validated for: {context.Principal?.Identity?.Name ?? "Unknown"}");
            return Task.CompletedTask;
        }
    };
});

// Configure Authorization with Permission-based policies
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddAuthorization(options =>
{
    // Base role policies
    options.AddPolicy("ProjectManager", policy => policy.RequireRole("ProjectManager"));
    options.AddPolicy("SubPMO", policy => policy.RequireRole("SubPMO"));
    options.AddPolicy("MainPMO", policy => policy.RequireRole("MainPMO"));
    options.AddPolicy("DepartmentDirector", policy => policy.RequireRole("DepartmentDirector"));
    options.AddPolicy("Executive", policy => policy.RequireRole("Executive"));
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    
    // Permission-based policies
    options.AddPolicy("Permission:ManageProjects", policy => 
        policy.Requirements.Add(new PermissionRequirement("ManageProjects")));
    options.AddPolicy("Permission:ApproveRequests", policy => 
        policy.Requirements.Add(new PermissionRequirement("ApproveRequests")));
    options.AddPolicy("Permission:ManageUsers", policy => 
        policy.Requirements.Add(new PermissionRequirement("ManageUsers")));
    options.AddPolicy("Permission:ViewReports", policy => 
        policy.Requirements.Add(new PermissionRequirement("ViewReports")));
    options.AddPolicy("Permission:ViewAuditLogs", policy => 
        policy.Requirements.Add(new PermissionRequirement("ViewAuditLogs")));
    
    // IMPORTANT: Configure a fallback policy for development
    // This makes authentication optional but still available through User.Identity.IsAuthenticated
    if (builder.Environment.IsDevelopment())
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAssertion(_ => true) // Always succeed
            .Build();
    }
});

// Add Hangfire with in-memory storage for development
builder.Services.AddHangfire(config =>
{
    config.UseMemoryStorage();
});
builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowedOrigins");

// Middleware to log request details for debugging
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();
        Console.WriteLine($"API Request: {context.Request.Method} {context.Request.Path}");
        Console.WriteLine($"Auth Header: {(string.IsNullOrEmpty(authHeader) ? "None" : $"{authHeader[..Math.Min(20, authHeader.Length)]}...")}");
        
        // Check for token in query string
        var queryToken = context.Request.Query["auth_token"].ToString();
        if (!string.IsNullOrEmpty(queryToken))
        {
            Console.WriteLine($"Query token: {queryToken[..Math.Min(10, queryToken.Length)]}...");
            // Add it to headers if not present
            if (string.IsNullOrEmpty(authHeader))
            {
                context.Request.Headers["Authorization"] = $"Bearer {queryToken}";
            }
        }
    }
    
    await next();
});

app.UseAuthentication();

// Add our custom authentication middleware
app.UseMiddleware<EnterprisePMO_PWA.Web.Middleware.AuthenticationMiddleware>();

app.UseAuthorization();

// Map controllers - for development direct to Dashboard
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// SignalR hub for notifications
app.MapHub<NotificationHub>("/notificationHub");

// Add Hangfire dashboard with simplified auth for development
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new EnterprisePMO_PWA.Web.Filters.DashboardAuthorizationFilter(builder.Configuration) }
});

// Seed data with more robust error handling
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        Console.WriteLine("Seeding database...");
        // Make sure we're recreating the in-memory database if it exists
        if (app.Environment.IsDevelopment() && dbContext.Database.IsInMemory())
        {
            Console.WriteLine("Recreating in-memory database...");
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        }
        
        DataSeeder.Seed(dbContext);
        Console.WriteLine("Database seeding completed successfully");
        
        // Create admin user if it doesn't exist
        var adminUser = dbContext.Users.FirstOrDefault(u => u.Username == "admin@test.com");
        if (adminUser == null)
        {
            Console.WriteLine("Creating admin test user...");
            
            // First ensure we have an admin department
            var adminDept = dbContext.Departments.FirstOrDefault(d => d.Name == "Administration");
            if (adminDept == null)
            {
                adminDept = new EnterprisePMO_PWA.Domain.Entities.Department
                {
                    Id = Guid.NewGuid(),
                    Name = "Administration"
                };
                dbContext.Departments.Add(adminDept);
                await dbContext.SaveChangesAsync();
            }
            
            // Then create the admin user
            adminUser = new EnterprisePMO_PWA.Domain.Entities.User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), // Fixed ID for easy reference
                Username = "admin@test.com",
                Role = EnterprisePMO_PWA.Domain.Entities.RoleType.Admin,
                DepartmentId = adminDept.Id,
                SupabaseId = "test-admin-id"
            };
            dbContext.Users.Add(adminUser);
            await dbContext.SaveChangesAsync();
            
            Console.WriteLine($"Admin test user created with ID: {adminUser.Id}");
        }
        else
        {
            Console.WriteLine($"Admin test user already exists with ID: {adminUser.Id}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding database: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}

Console.WriteLine("Starting web server on http://localhost:7000 and https://localhost:7001");
app.Run();
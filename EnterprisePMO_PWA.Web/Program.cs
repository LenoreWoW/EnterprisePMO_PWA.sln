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

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", corsBuilder =>
    {
        corsBuilder.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
    });
});

// JWT Authentication with detailed debugging
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? 
                  Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
                  "your-temporary-secret-key-for-testing-only-make-it-at-least-32-chars";

Console.WriteLine($"JWT Secret Key (first 10 chars): {jwtSecretKey.Substring(0, Math.Min(10, jwtSecretKey.Length))}...");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false, // Set to false for testing
        ValidateAudience = false, // Set to false for testing
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero
    };
    
    // Add detailed logging for JWT authentication
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"JWT Authentication Failed: {context.Exception.Message}");
            Console.WriteLine($"JWT Auth Failure Stack Trace: {context.Exception.StackTrace}");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            Console.WriteLine($"JWT Message Received Path: {context.Request.Path}");
            Console.WriteLine($"JWT Message Token: {context.Token ?? "No token"}");
            
            if (string.IsNullOrEmpty(context.Token))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                Console.WriteLine($"Authorization Header: {authHeader}");
                
                if (string.IsNullOrEmpty(authHeader))
                {
                    // Check for token in query string (for debugging only)
                    var queryToken = context.Request.Query["auth_token"].ToString();
                    if (!string.IsNullOrEmpty(queryToken))
                    {
                        Console.WriteLine($"Found token in query string: {queryToken.Substring(0, Math.Min(20, queryToken.Length))}...");
                        context.Token = queryToken;
                    }
                }
            }
            
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"JWT Token Validated: {context.Principal?.Identity?.Name ?? "Unknown"}");
            Console.WriteLine($"Token Claims: {string.Join(", ", context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>())}");
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

// Middleware to log request details
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request Path: {context.Request.Path}");
    Console.WriteLine($"Request Method: {context.Request.Method}");
    Console.WriteLine($"Authorization Header: {context.Request.Headers["Authorization"]}");
    
    await next.Invoke();
});

app.UseAuthentication();

// Comment out for testing
// app.UseAuthenticationSync();

app.UseAuthorization();

// MODIFIED: Change default route to TestController for debugging
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Test}/{action=Index}/{id?}");

// SignalR hub for notifications
app.MapHub<NotificationHub>("/notificationHub");

// Add Hangfire dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new EnterprisePMO_PWA.Web.Filters.DashboardAuthorizationFilter(builder.Configuration) }
});

// Seed data
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
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding database: {ex.Message}");
    }
}

Console.WriteLine("Starting web server on http://localhost:7000 and https://localhost:7001");
app.Run();
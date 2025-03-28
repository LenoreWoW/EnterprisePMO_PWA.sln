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

// JWT Authentication
var supabaseProjectRef = builder.Configuration["Supabase:ProjectRef"];
var supabaseJwtSecret = builder.Configuration["Supabase:JwtSecret"];
var supabaseIssuer = $"https://{supabaseProjectRef}.supabase.co";
var supabaseAudience = supabaseProjectRef;

if (string.IsNullOrEmpty(supabaseJwtSecret))
{
    supabaseJwtSecret = builder.Configuration["SUPABASE_JWT_SECRET"] ?? 
                        Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = supabaseIssuer,
        ValidAudience = supabaseAudience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(supabaseJwtSecret ?? "fallback-dev-key-not-for-production")),
        // Add the following to handle clock skew
        ClockSkew = TimeSpan.Zero
    };
    
    // Add event handlers for successful token validation and token validation failure
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            // This is called when the token is successfully validated
            Console.WriteLine($"Token validated for user: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            // This is called when the authentication fails
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
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

app.UseAuthentication();
app.UseAuthenticationSync(); // Add our custom authentication middleware
app.UseAuthorization();

// Map controllers with a new default route to Account/Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

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
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EnterprisePMO_PWA.Infrastructure.Services;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Web.Hubs;
using EnterprisePMO_PWA.Web.Services;
using EnterprisePMO_PWA.Web.Authorization;
using EnterprisePMO_PWA.Web.Extensions;
using EnterprisePMO_PWA.Domain.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Hangfire;
using Hangfire.MemoryStorage;
using System.Text;
using System.Security.Claims;

// Setup the web server
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

// Configure Supabase
builder.Services.AddHttpClient<SupabaseClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Supabase:Url"] ?? 
        throw new InvalidOperationException("Supabase URL is not configured"));
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

// Register authentication services
builder.Services.AddScoped<IAuthService, AuthService>();

// Register new workflow and notification services
builder.Services.AddScoped<ProjectWorkflowService>();

// Add SignalR
builder.Services.AddSignalR();

// Register real-time notification service (Web layer)
builder.Services.AddScoped<IRealTimeNotificationService, SignalRNotificationService>();

// Register notification service (Application layer)
builder.Services.AddScoped<INotificationService, EnhancedNotificationService>();

// Add notification services
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<PushNotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<EmailDigestService>();

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

// Configure authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Supabase:Url"];
    options.Audience = builder.Configuration["Supabase:Key"];
    options.RequireHttpsMetadata = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.FromMinutes(5)
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

// Add Gantt service
builder.Services.AddScoped<GanttService>();

// Add PDF export service
builder.Services.AddScoped<PdfExportService>();

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
            Console.WriteLine($"Query token found: {queryToken[..Math.Min(10, queryToken.Length)]}...");
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

Console.WriteLine("Starting web server on http://localhost:7000 and https://localhost:7001");
app.Run();
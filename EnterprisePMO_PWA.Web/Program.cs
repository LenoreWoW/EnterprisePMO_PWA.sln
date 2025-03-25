using EnterprisePMO_PWA.Infrastructure.Data;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Web.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Dashboard;
using System.Text;

// Force IPv4 - Add this at the top of the file
AppContext.SetSwitch("System.Net.DisableIPv6", true);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    // Configure Serilog
    builder.Host.UseSerilog((hostingContext, loggerConfiguration) => 
    {
        loggerConfiguration
            .ReadFrom.Configuration(hostingContext.Configuration)
            .Enrich.FromLogContext();
    });

    // Add services to the container
    // Database with IPv6 disabled by modifying the connection string
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!connectionString.Contains("IPv4Only"))
    {
        connectionString += ";IPv4Only=true";
    }
    
    builder.Services.AddDbContext<AppDbContext>(options => 
        options.UseNpgsql(connectionString));

    // Register services
    builder.Services.AddScoped<ProjectService>();
    builder.Services.AddScoped<WeeklyUpdateService>();
    builder.Services.AddScoped<ChangeRequestService>();
    builder.Services.AddScoped<RoleService>();
    builder.Services.AddScoped<DepartmentService>();
    builder.Services.AddScoped<ProjectMemberService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<WorkflowService>();
    builder.Services.AddScoped<AuditService>();
    builder.Services.AddHttpContextAccessor();

    // MVC & API
    builder.Services.AddControllersWithViews()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
            options.JsonSerializerOptions.WriteIndented = true;
        });

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowedOrigins", corsBuilder =>
        {
            var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "https://localhost:5001" };
            corsBuilder.WithOrigins(origins)
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
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
        
        if (string.IsNullOrEmpty(supabaseJwtSecret))
        {
            Log.Warning("JWT secret not found in configuration or environment variables");
        }
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
                Encoding.UTF8.GetBytes(supabaseJwtSecret ?? "fallback-dev-key-not-for-production"))
        };
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("ProjectManager", policy => policy.RequireRole("ProjectManager"));
        options.AddPolicy("SubPMO", policy => policy.RequireRole("SubPMO"));
        options.AddPolicy("MainPMO", policy => policy.RequireRole("MainPMO"));
        options.AddPolicy("DepartmentDirector", policy => policy.RequireRole("DepartmentDirector"));
        options.AddPolicy("Executive", policy => policy.RequireRole("Executive"));
        options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    });

    // Hangfire for background jobs
    builder.Services.AddHangfire(configuration =>
    {
        configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                     .UseSimpleAssemblyNameTypeSerializer()
                     .UseRecommendedSerializerSettings()
                     .UsePostgreSqlStorage(connectionString);
    });
    builder.Services.AddHangfireServer();

    // SignalR for real-time notifications
    builder.Services.AddSignalR();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseCors("AllowedOrigins");

    app.UseAuthentication();
    app.UseAuthorization();

    // Request logging middleware
    app.Use(async (context, next) =>
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        await next();
        watch.Stop();
        Log.Information($"[{context.Request.Method}] {context.Request.Path} took {watch.ElapsedMilliseconds} ms");
    });

    // Seed initial data
    using (var scope = app.Services.CreateScope())
    {
        try 
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Log.Information("Seeding database...");
            DataSeeder.Seed(dbContext);
            Log.Information("Database seeding completed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while seeding the database: {ErrorMessage}", ex.Message);
            // Continue running the app even if seeding fails
        }
    }

    // Map controllers
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Hangfire dashboard
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new EnterprisePMO_PWA.Web.Filters.DashboardAuthorizationFilter(builder.Configuration) }
    });

    // SignalR hub
    app.MapHub<NotificationHub>("/notificationHub");

    Log.Information("Starting web host");
    app.Run();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
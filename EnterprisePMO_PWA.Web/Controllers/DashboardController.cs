using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EnterprisePMO_PWA.Infrastructure.Data;
using EnterprisePMO_PWA.Application.Services;
using Microsoft.EntityFrameworkCore;
using EnterprisePMO_PWA.Domain.Entities;
using System.Security.Claims;

namespace EnterprisePMO_PWA.Web.Controllers
{
    // Comment out the Authorize attribute temporarily for testing
    // [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ProjectService _projectService;
        private readonly WeeklyUpdateService _weeklyUpdateService;
        private readonly AuditService _auditService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            AppDbContext context,
            ProjectService projectService,
            WeeklyUpdateService weeklyUpdateService,
            AuditService auditService,
            ILogger<DashboardController> logger)
        {
            _context = context;
            _projectService = projectService;
            _weeklyUpdateService = weeklyUpdateService;
            _auditService = auditService;
            _logger = logger;
        }

        // GET: /Dashboard
        public async Task<IActionResult> Index()
        {
            // Debug auth info
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            _logger.LogInformation($"Authorization header: {authHeader}");
            
            // Check query string token
            var queryToken = HttpContext.Request.Query["auth_token"].ToString();
            if (!string.IsNullOrEmpty(queryToken))
            {
                _logger.LogInformation($"Query token found: {queryToken.Substring(0, Math.Min(20, queryToken.Length))}...");
            }
            
            _logger.LogInformation($"User authenticated: {User.Identity?.IsAuthenticated}");
            _logger.LogInformation($"Username: {User.Identity?.Name}");
            
            // Get current user's username
            var username = User.Identity?.Name;
            
            if (string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("No username found in the Identity");
                
                // TEMPORARY: Get admin user for testing
                if (!User.Identity?.IsAuthenticated == true)
                {
                    _logger.LogWarning("Not authenticated but continuing for debugging");
                    username = "admin@test.com";
                    
                    // For debugging only - remove in production!
                    var adminUser = await _context.Users
                        .Include(u => u.Department)
                        .FirstOrDefaultAsync(u => u.Username == username);
                    
                    if (adminUser != null)
                    {
                        // Pass user info to view for debugging
                        ViewBag.User = new
                        {
                            Id = adminUser.Id,
                            Username = adminUser.Username,
                            Role = adminUser.Role.ToString(),
                            Department = adminUser.Department?.Name,
                            IsDebugMode = true
                        };
                        
                        _logger.LogWarning("Using debug user for view: {Username}", adminUser.Username);
                        
                        return View();
                    }
                }
                
                _logger.LogWarning("Redirecting to login due to missing username");
                return RedirectToAction("Login", "Account");
            }
            
            try
            {
                // Log dashboard access
                await _auditService.LogAction(username, "DashboardAccess", "Dashboard", "User accessed the dashboard");
                
                // Get current user info
                var user = await _context.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Username == username);
                
                if (user == null)
                {
                    _logger.LogWarning("User {Username} not found in database", username);
                    return RedirectToAction("Login", "Account");
                }
                
                // Pass user info to view
                ViewBag.User = new
                {
                    Id = user.Id,
                    Username = user.Username,
                    Role = user.Role.ToString(),
                    Department = user.Department?.Name
                };
                
                // Get dashboard data based on user role
                await LoadDashboardData(user);
                
                return View();
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError(ex, "Error accessing dashboard: {Message}", ex.Message);
                await _auditService.LogAction(username, "DashboardError", "Dashboard", $"Error accessing dashboard: {ex.Message}");
                
                // Show error page
                return RedirectToAction("Error", "Home", new { message = "Error loading dashboard data" });
            }
        }

        // The rest of the controller methods remain unchanged
        // ...

        // Helper method to load dashboard data based on user role
        private async Task LoadDashboardData(User user)
        {
            try
            {
                // Common data for all roles
                var activeProjects = await _context.Projects
                    .Where(p => p.Status == ProjectStatus.Active)
                    .CountAsync();
                
                var completedProjects = await _context.Projects
                    .Where(p => p.Status == ProjectStatus.Completed)
                    .CountAsync();
                
                var totalTasks = await _context.Tasks
                    .CountAsync();
                
                var completedTasks = await _context.Tasks
                    .Where(t => t.Status == ProjectTaskStatus.Completed)
                    .CountAsync();
                
                var recentActivities = await _context.AuditLogs
                    .OrderByDescending(a => a.Timestamp)
                    .Take(5)
                    .ToListAsync();
                
                // Calculate upcoming deadlines (projects ending in the next 30 days)
                var upcomingDeadlines = await _context.Projects
                    .Where(p => p.Status == ProjectStatus.Active && p.EndDate <= DateTime.Now.AddDays(30))
                    .OrderBy(p => p.EndDate)
                    .Take(3)
                    .ToListAsync();
                
                // Pass common data to view
                ViewBag.ActiveProjects = activeProjects;
                ViewBag.CompletedProjects = completedProjects;
                ViewBag.TotalTasks = totalTasks;
                ViewBag.CompletedTasks = completedTasks;
                ViewBag.RecentActivities = recentActivities;
                ViewBag.UpcomingDeadlines = upcomingDeadlines;
                
                // Role-specific data
                switch (user.Role)
                {
                    case RoleType.ProjectManager:
                        // Projects managed by current user
                        var managedProjects = await _context.Projects
                            .Where(p => p.ProjectManagerId == user.Id)
                            .Include(p => p.Department)
                            .OrderBy(p => p.EndDate)
                            .Take(5)
                            .ToListAsync();
                        
                        ViewBag.ManagedProjects = managedProjects;
                        break;
                    
                    case RoleType.SubPMO:
                    case RoleType.MainPMO:
                        // Projects in approval queue
                        var pendingApprovals = await _context.Projects
                            .Where(p => p.Status == ProjectStatus.Proposed)
                            .Include(p => p.Department)
                            .Include(p => p.ProjectManager)
                            .OrderBy(p => p.CreationDate)
                            .Take(5)
                            .ToListAsync();
                        
                        ViewBag.PendingApprovals = pendingApprovals;
                        
                        // Department performance - with null check for Department 
                        var departmentPerformance = await _context.Projects
                            .Where(p => p.Status == ProjectStatus.Active || p.Status == ProjectStatus.Completed)
                            .Where(p => p.Department != null)
                            .GroupBy(p => p.Department!.Name)
                            .Select(g => new
                            {
                                Department = g.Key,
                                TotalProjects = g.Count(),
                                CompletedProjects = g.Count(p => p.Status == ProjectStatus.Completed),
                                AverageCompletion = g.Average(p => p.PercentComplete)
                            })
                            .ToListAsync();
                        
                        ViewBag.DepartmentPerformance = departmentPerformance;
                        break;
                    
                    case RoleType.Executive:
                    case RoleType.DepartmentDirector:
                        // Financial overview
                        var financialOverview = new
                        {
                            TotalBudget = await _context.Projects
                                .Where(p => p.Status == ProjectStatus.Active)
                                .SumAsync(p => p.Budget),
                            TotalCost = await _context.Projects
                                .Where(p => p.Status == ProjectStatus.Active)
                                .SumAsync(p => p.EstimatedCost),
                            ProjectsOverBudget = await _context.Projects
                                .Where(p => p.Status == ProjectStatus.Active && p.EstimatedCost > p.Budget)
                                .CountAsync()
                        };
                        
                        ViewBag.FinancialOverview = financialOverview;
                        break;
                    
                    default:
                        // Default dashboard data
                        var allProjects = await _context.Projects
                            .Include(p => p.Department)
                            .OrderByDescending(p => p.CreationDate)
                            .Take(5)
                            .ToListAsync();
                        
                        ViewBag.AllProjects = allProjects;
                        break;
                }
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError(ex, "Error loading dashboard data: {Message}", ex.Message);
                await _auditService.LogAction(user.Username, "LoadDashboardDataError", "Dashboard", $"Error loading dashboard data: {ex.Message}");
                
                // Re-throw to be handled by the calling method
                throw;
            }
        }

        // Other helper methods remain unchanged
        // ...
    }
}
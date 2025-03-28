using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EnterprisePMO_PWA.Infrastructure.Data;
using EnterprisePMO_PWA.Application.Services;
using Microsoft.EntityFrameworkCore;
using EnterprisePMO_PWA.Domain.Entities;
using System.Security.Claims;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ProjectService _projectService;
        private readonly WeeklyUpdateService _weeklyUpdateService;
        private readonly AuditService _auditService;

        public DashboardController(
            AppDbContext context,
            ProjectService projectService,
            WeeklyUpdateService weeklyUpdateService,
            AuditService auditService)
        {
            _context = context;
            _projectService = projectService;
            _weeklyUpdateService = weeklyUpdateService;
            _auditService = auditService;
        }

        // GET: /Dashboard
        public async Task<IActionResult> Index()
        {
            // Get current user's username
            var username = User.Identity?.Name;
            
            if (string.IsNullOrEmpty(username))
            {
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
                await _auditService.LogAction(username, "DashboardError", "Dashboard", $"Error accessing dashboard: {ex.Message}");
                
                // Show error page
                return RedirectToAction("Error", "Home", new { message = "Error loading dashboard data" });
            }
        }

        // GET: /Dashboard/Executive
        [Authorize(Roles = "Executive,Admin,MainPMO")]
        public async Task<IActionResult> Executive()
        {
            // Get current user's username
            var username = User.Identity?.Name;
            
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }
            
            try
            {
                // Log executive dashboard access
                await _auditService.LogAction(username, "ExecutiveDashboardAccess", "Dashboard", "User accessed the executive dashboard");
                
                // Get all active projects for executive view
                var projects = await _projectService.GetAllActiveProjects();
                
                // Calculate metrics
                var metrics = CalculateExecutiveMetrics(projects);
                
                // Pass data to view
                ViewBag.Projects = projects;
                ViewBag.Metrics = metrics;
                
                return View();
            }
            catch (Exception ex)
            {
                // Log error
                await _auditService.LogAction(username, "ExecutiveDashboardError", "Dashboard", $"Error accessing executive dashboard: {ex.Message}");
                
                // Show error page
                return RedirectToAction("Error", "Home", new { message = "Error loading executive dashboard data" });
            }
        }

        // GET: /Dashboard/ProjectManager
        [Authorize(Roles = "ProjectManager,SubPMO,MainPMO,Admin")]
        public async Task<IActionResult> ProjectManager()
        {
            // Get current user's username
            var username = User.Identity?.Name;
            
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }
            
            try
            {
                // Log project manager dashboard access
                await _auditService.LogAction(username, "ProjectManagerDashboardAccess", "Dashboard", "User accessed the project manager dashboard");
                
                // Get current user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                
                // Get projects managed by current user
                var projects = await _projectService.GetProjectsByManager(user.Id);
                
                // Get recent updates
                var recentUpdates = await _weeklyUpdateService.GetRecentUpdates(5);
                
                // Pass data to view
                ViewBag.Projects = projects;
                ViewBag.RecentUpdates = recentUpdates;
                
                return View();
            }
            catch (Exception ex)
            {
                // Log error
                await _auditService.LogAction(username, "ProjectManagerDashboardError", "Dashboard", $"Error accessing project manager dashboard: {ex.Message}");
                
                // Show error page
                return RedirectToAction("Error", "Home", new { message = "Error loading project manager dashboard data" });
            }
        }

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
                        
                        // Department performance - simplified version without StrategicGoal 
                        var departmentPerformance = await _context.Projects
                            .Where(p => p.Status == ProjectStatus.Active || p.Status == ProjectStatus.Completed)
                            .GroupBy(p => p.Department.Name)
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
                await _auditService.LogAction(user.Username, "LoadDashboardDataError", "Dashboard", $"Error loading dashboard data: {ex.Message}");
                
                // Re-throw to be handled by the calling method
                throw;
            }
        }

        // Helper method to calculate executive metrics
        private object CalculateExecutiveMetrics(List<Project> projects)
        {
            // Default metrics for development
            if (projects == null || !projects.Any())
            {
                return new
                {
                    TotalProjects = 0,
                    OnTrack = 0,
                    AtRisk = 0,
                    Delayed = 0,
                    AverageCompletion = 0,
                    BudgetUtilization = 0
                };
            }

            // Calculate actual metrics from projects
            var onTrack = projects.Count(p => p.StatusColor == "Green");
            var atRisk = projects.Count(p => p.StatusColor == "Yellow");
            var delayed = projects.Count(p => p.StatusColor == "Red");
            var averageCompletion = projects.Average(p => p.PercentComplete);
            
            // Budget utilization
            var totalBudget = projects.Sum(p => p.Budget);
            var totalCost = projects.Sum(p => p.EstimatedCost);
            var budgetUtilization = totalBudget > 0 ? (totalCost / totalBudget) * 100 : 0;

            return new
            {
                TotalProjects = projects.Count,
                OnTrack = onTrack,
                AtRisk = atRisk,
                Delayed = delayed,
                AverageCompletion = Math.Round(averageCompletion, 1),
                BudgetUtilization = Math.Round(budgetUtilization, 1)
            };
        }
    }
}
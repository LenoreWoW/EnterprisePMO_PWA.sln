using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EnterprisePMO_PWA.Infrastructure.Data;
using EnterprisePMO_PWA.Application.Services;
using Microsoft.EntityFrameworkCore;
using EnterprisePMO_PWA.Domain.Entities;
using System.Security.Claims;
using System;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Enums;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ProjectService _projectService;
        private readonly WeeklyUpdateService _weeklyUpdateService;
        private readonly AuditService _auditService;
        private readonly ILogger<DashboardController> _logger;
        private readonly NotificationService _notificationService;
        private readonly IAuthService _authService;

        public DashboardController(
            AppDbContext context,
            ProjectService projectService,
            WeeklyUpdateService weeklyUpdateService,
            AuditService auditService,
            ILogger<DashboardController> logger,
            NotificationService notificationService,
            IAuthService authService)
        {
            _context = context;
            _projectService = projectService;
            _weeklyUpdateService = weeklyUpdateService;
            _auditService = auditService;
            _logger = logger;
            _notificationService = notificationService;
            _authService = authService;
        }

        // GET: /Dashboard
        public async Task<IActionResult> Index()
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            var viewModel = new DashboardViewModel
            {
                RecentProjects = await GetRecentProjects(currentUser.Role),
                RecentUpdates = await _weeklyUpdateService.GetRecentAsync(5),
                Notifications = await _notificationService.GetUserNotificationsAsync(currentUser.Id),
                ProjectStats = await GetProjectStats(currentUser.Role)
            };

            return View(viewModel);
        }

        private async Task<IEnumerable<Project>> GetRecentProjects(UserRole userRole)
        {
            var projects = await _projectService.GetAllAsync();
            
            switch (userRole)
            {
                case UserRole.Admin:
                    return projects.OrderByDescending(p => p.UpdatedAt).Take(5);
                case UserRole.PMOHead:
                    return projects.Where(p => p.Status == ProjectStatus.PendingApproval)
                                 .OrderByDescending(p => p.UpdatedAt)
                                 .Take(5);
                case UserRole.ProjectManager:
                    var currentUser = await _authService.GetCurrentUserAsync();
                    return projects.Where(p => p.ProjectManagerId == currentUser.Id)
                                 .OrderByDescending(p => p.UpdatedAt)
                                 .Take(5);
                default:
                    currentUser = await _authService.GetCurrentUserAsync();
                    return projects.Where(p => p.ProjectMembers.Any(pm => pm.UserId == currentUser.Id))
                                 .OrderByDescending(p => p.UpdatedAt)
                                 .Take(5);
            }
        }

        private async Task<ProjectStats> GetProjectStats(UserRole userRole)
        {
            var projects = await _projectService.GetAllAsync();
            
            switch (userRole)
            {
                case UserRole.Admin:
                    return new ProjectStats
                    {
                        Total = projects.Count(),
                        Active = projects.Count(p => p.Status == ProjectStatus.Active),
                        Completed = projects.Count(p => p.Status == ProjectStatus.Completed),
                        OnHold = projects.Count(p => p.Status == ProjectStatus.OnHold)
                    };
                case UserRole.PMOHead:
                    return new ProjectStats
                    {
                        Total = projects.Count(p => p.Status == ProjectStatus.PendingApproval),
                        Active = projects.Count(p => p.Status == ProjectStatus.Active),
                        Completed = projects.Count(p => p.Status == ProjectStatus.Completed),
                        OnHold = projects.Count(p => p.Status == ProjectStatus.OnHold)
                    };
                case UserRole.ProjectManager:
                    var currentUser = await _authService.GetCurrentUserAsync();
                    return new ProjectStats
                    {
                        Total = projects.Count(p => p.ProjectManagerId == currentUser.Id),
                        Active = projects.Count(p => p.ProjectManagerId == currentUser.Id && p.Status == ProjectStatus.Active),
                        Completed = projects.Count(p => p.ProjectManagerId == currentUser.Id && p.Status == ProjectStatus.Completed),
                        OnHold = projects.Count(p => p.ProjectManagerId == currentUser.Id && p.Status == ProjectStatus.OnHold)
                    };
                default:
                    currentUser = await _authService.GetCurrentUserAsync();
                    return new ProjectStats
                    {
                        Total = projects.Count(p => p.ProjectMembers.Any(pm => pm.UserId == currentUser.Id)),
                        Active = projects.Count(p => p.ProjectMembers.Any(pm => pm.UserId == currentUser.Id) && p.Status == ProjectStatus.Active),
                        Completed = projects.Count(p => p.ProjectMembers.Any(pm => pm.UserId == currentUser.Id) && p.Status == ProjectStatus.Completed),
                        OnHold = projects.Count(p => p.ProjectMembers.Any(pm => pm.UserId == currentUser.Id) && p.Status == ProjectStatus.OnHold)
                    };
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

    public class DashboardViewModel
    {
        public IEnumerable<Project> RecentProjects { get; set; }
        public IEnumerable<WeeklyUpdate> RecentUpdates { get; set; }
        public IEnumerable<Notification> Notifications { get; set; }
        public ProjectStats ProjectStats { get; set; }
    }

    public class ProjectStats
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Completed { get; set; }
        public int OnHold { get; set; }
    }
}
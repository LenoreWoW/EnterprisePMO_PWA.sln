using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Enums;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnterprisePMO_PWA.Application.Services
{
    /// <summary>
    /// Provides business logic for managing projects.
    /// </summary>
    public class ProjectService
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public ProjectService(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        /// <summary>
        /// Creates a new project.
        /// </summary>
        public async Task<Project> CreateProjectAsync(Project project)
        {
            project.Id = Guid.NewGuid();
            project.CreationDate = DateTime.UtcNow;
            project.Status = ProjectStatus.Draft;

            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            return project;
        }

        /// <summary>
        /// Retrieves a project by ID.
        /// </summary>
        public Project? GetProjectById(Guid id)
        {
            return _context.Projects.FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// Updates an existing project.
        /// </summary>
        public async Task<Project> UpdateProjectAsync(Project project)
        {
            var existingProject = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == project.Id);

            if (existingProject == null)
            {
                throw new KeyNotFoundException($"Project with ID {project.Id} not found.");
            }

            // Update basic properties
            existingProject.Name = project.Name;
            existingProject.Description = project.Description;
            existingProject.StartDate = project.StartDate;
            existingProject.EndDate = project.EndDate;
            existingProject.Budget = project.Budget;
            existingProject.ActualCost = project.ActualCost;
            existingProject.EstimatedCost = project.EstimatedCost;
            existingProject.ClientName = project.ClientName;
            existingProject.Category = project.Category;
            existingProject.PercentComplete = project.PercentComplete;
            existingProject.Status = project.Status;
            existingProject.StatusColor = project.StatusColor;
            existingProject.DepartmentId = project.DepartmentId;
            existingProject.ProjectManagerId = project.ProjectManagerId;
            existingProject.StrategicGoalId = project.StrategicGoalId;
            existingProject.AnnualGoalId = project.AnnualGoalId;

            // Update project members
            if (project.Members != null)
            {
                // Remove members that are not in the updated list
                var membersToRemove = existingProject.Members
                    .Where(m => !project.Members.Any(pm => pm.Id == m.Id))
                    .ToList();

                foreach (var member in membersToRemove)
                {
                    existingProject.Members.Remove(member);
                }

                // Add new members
                foreach (var member in project.Members)
                {
                    if (!existingProject.Members.Any(m => m.Id == member.Id))
                    {
                        existingProject.Members.Add(member);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return existingProject;
        }

        /// <summary>
        /// Deletes a project.
        /// </summary>
        public async Task DeleteProjectAsync(Guid id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Gets all active projects.
        /// </summary>
        public async Task<List<Project>> GetAllActiveProjects()
        {
            return await _context.Projects
                .Where(p => p.Status == ProjectStatus.Active)
                .Include(p => p.Department)
                .Include(p => p.ProjectManager)
                .ToListAsync();
        }

        /// <summary>
        /// Gets projects managed by a specific user.
        /// </summary>
        public async Task<List<Project>> GetProjectsByManager(Guid managerId)
        {
            return await _context.Projects
                .Where(p => p.ProjectManagerId == managerId)
                .Include(p => p.Department)
                .ToListAsync();
        }
    }
}
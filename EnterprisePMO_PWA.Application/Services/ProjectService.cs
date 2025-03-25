using System;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
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

        public ProjectService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new project.
        /// </summary>
        public async Task<Project> CreateProjectAsync(Project project)
        {
            project.Id = Guid.NewGuid();
            project.CreationDate = DateTime.UtcNow;
            project.StatusColor = project.ComputeStatusColor();
            project.Status = ProjectStatus.Proposed;
            _context.Projects.Add(project);
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
        public async Task UpdateProjectAsync(Project project)
        {
            project.StatusColor = project.ComputeStatusColor();
            _context.Entry(project).State = EntityState.Modified;
            await _context.SaveChangesAsync();
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnterprisePMO_PWA.Application.Services
{
    /// <summary>
    /// Provides methods for managing departments.
    /// </summary>
    public class DepartmentService
    {
        private readonly AppDbContext _context;

        public DepartmentService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new department.
        /// </summary>
        public async Task<Department> CreateDepartmentAsync(Department department)
        {
            department.Id = Guid.NewGuid();
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            return department;
        }

        /// <summary>
        /// Retrieves all departments.
        /// </summary>
        public List<Department> GetAllDepartments()
        {
            return _context.Departments.ToList();
        }

        /// <summary>
        /// Updates an existing department.
        /// </summary>
        public async Task<Department?> UpdateDepartmentAsync(Department department)
        {
            _context.Entry(department).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return department;
        }

        /// <summary>
        /// Deletes a department.
        /// </summary>
        public async Task DeleteDepartmentAsync(Guid id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept != null)
            {
                _context.Departments.Remove(dept);
                await _context.SaveChangesAsync();
            }
        }
    }
}

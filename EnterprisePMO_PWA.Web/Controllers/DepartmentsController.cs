using System;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepartmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Departments
        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Departments
                .Where(d => d.Name != "Holding") // Exclude Holding department from signup
                .Select(d => new { d.Id, d.Name })
                .OrderBy(d => d.Name)
                .ToListAsync();

            return Ok(departments);
        }
    }
}
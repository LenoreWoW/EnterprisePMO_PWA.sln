using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TestController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }

        [HttpGet("db")]
        public async Task<IActionResult> DatabaseCheck()
        {
            try
            {
                // Try to query the database
                var departments = await _context.Departments.ToListAsync();
                
                return Ok(new { 
                    status = "connected", 
                    departmentCount = departments.Count,
                    message = "Database connection successful"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    status = "error", 
                    message = $"Database connection failed: {ex.Message}" 
                });
            }
        }

        [HttpGet("exception")]
        public IActionResult ThrowException()
        {
            throw new Exception("This is a test exception for error handling testing");
        }

        [HttpGet("html")]
        public ContentResult GetHtml()
        {
            string html = $@"
            <!DOCTYPE html>
            <html>
                <head>
                    <title>Test HTML Response</title>
                </head>
                <body>
                    <h1>HTML Test</h1>
                    <p>This is a test HTML response from the API.</p>
                    <p>Server time: {DateTime.UtcNow}</p>
                </body>
            </html>";

            return Content(html, "text/html");
        }

        [HttpGet("debug-info")]
        public IActionResult GetDebugInfo()
        {
            var environmentInfo = new
            {
                MachineName = Environment.MachineName,
                OSVersion = Environment.OSVersion.VersionString,
                ProcessorCount = Environment.ProcessorCount,
                FrameworkDescription = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                TimeZone = TimeZoneInfo.Local.DisplayName
            };

            return Ok(environmentInfo);
        }
    }
}
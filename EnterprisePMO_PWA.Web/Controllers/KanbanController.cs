using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EnterprisePMO_PWA.Web.Controllers
{
    /// <summary>
    /// Provides endpoints for Kanban board functionality.
    /// </summary>
    [ApiController]
    [Route("api/kanban")]
    public class KanbanController : ControllerBase
    {
        private readonly ILogger<KanbanController> _logger;
        public KanbanController(ILogger<KanbanController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Receives an updated order of tasks from the Kanban board.
        /// </summary>
        [HttpPost("updateorder")]
        public Task<IActionResult> UpdateOrder([FromBody] KanbanOrderUpdate update)
        {
            // Here you would update the task order in the database.
            _logger.LogInformation("New Kanban order received: {Order}", update.Order);
            return Task.FromResult<IActionResult>(Ok(new { message = "Order updated successfully", order = update.Order }));
        }
    }

    /// <summary>
    /// Model for Kanban board order updates.
    /// </summary>
    public class KanbanOrderUpdate
    {
        public required string[] Order { get; set; }
    }
}
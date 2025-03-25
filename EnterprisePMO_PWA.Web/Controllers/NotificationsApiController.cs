using Microsoft.AspNetCore.Mvc;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        /// <summary>
        /// Dummy endpoint to simulate notification background sync.
        /// </summary>
        [HttpGet("sync")]
        public IActionResult SyncNotifications()
        {
            return Ok(new { message = "Notifications synced successfully." });
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace EnterprisePMO_PWA.Web.Controllers
{
    /// <summary>
    /// Provides dashboard views for standard and executive users.
    /// </summary>
    public class DashboardController : Controller
    {
        public IActionResult Index() => View();

        public IActionResult Executive() => View();
    }
}

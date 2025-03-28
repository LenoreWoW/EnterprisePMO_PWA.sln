using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace EnterprisePMO_PWA.Web.Controllers
{
    /// <summary>
    /// Handles account actions such as login and signup.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            _logger.LogInformation("Login page accessed");
            return View();
        }

        [HttpGet]
        public IActionResult Signup()
        {
            _logger.LogInformation("Signup page accessed");
            return View();
        }

        [HttpGet]
        public IActionResult DirectLogin()
        {
            _logger.LogInformation("DirectLogin page accessed");
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            _logger.LogInformation("ResetPassword page accessed");
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
            _logger.LogInformation("Profile page accessed");
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult Settings()
        {
            _logger.LogInformation("Settings page accessed");
            return View();
        }
    }
}
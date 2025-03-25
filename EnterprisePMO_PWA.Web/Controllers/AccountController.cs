using Microsoft.AspNetCore.Mvc;

namespace EnterprisePMO_PWA.Web.Controllers
{
    /// <summary>
    /// Handles account actions such as login and signup.
    /// </summary>
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login() => View();

        [HttpGet]
        public IActionResult Signup() => View();
    }
}

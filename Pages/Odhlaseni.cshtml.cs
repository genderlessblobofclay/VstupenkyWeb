using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using VstupenkyWeb.Models;
using System.Security.Claims;

namespace VstupenkyWeb.Pages
{
    public class OdhlaseniModel : PageModel
    {
        private readonly LoginManager _loginManager; // Inject your LoginManager

        public OdhlaseniModel(LoginManager loginManager)
        {
            _loginManager = loginManager;
        }

        public async Task<IActionResult> OnGet()
        {
            // Sign the user out
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirect to the login page
            return RedirectToPage("/Index");
        }
    }
}
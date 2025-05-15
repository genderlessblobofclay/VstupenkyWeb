using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using VstupenkyWeb.Models; // Assuming your models are in this namespace
using System.Security.Claims; // Add this for ClaimTypes

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
            // Get the current user's ID (you'll need to adapt this to your setup)
            var userId = GetCurrentUserId(); // Implement this method

            // Update the user's role to "Navstevnik" (1)
            if (userId != -1) // Ensure a valid user ID
            {
                _loginManager.UpdateUserRole(userId, (int)Role.Navstevnik); // Assuming Role.Navstevnik = 1
            }

            // Sign the user out
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirect to the login page
            return RedirectToPage("/Index");
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return -1; // Or throw an exception if no user is found
        }
    }
}
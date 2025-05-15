using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using VstupenkyWeb.Models; // Assuming your models are in this namespace
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace VstupenkyWeb.Pages.LoginInfo
{
    [AllowAnonymous]
    public class ZapomenuteHesloModel : PageModel
    {
        private readonly LoginManager _loginManager; // Inject your LoginManager

        public ZapomenuteHesloModel(LoginManager loginManager)
        {
            _loginManager = loginManager;
        }

        [BindProperty]
        [Required(ErrorMessage = "Email je povinný.")]
        [EmailAddress(ErrorMessage = "Neplatný formát emailu.")]
        public string Email { get; set; }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if the email exists in the database
            if (!_loginManager.EmailExists(Email))
            {
                ModelState.AddModelError("Email", "Uživatel s tímto emailem neexistuje.");
                return Page();
            }

            // Generate a password reset token
            string resetToken = _loginManager.GeneratePasswordResetToken(Email);

            // Send the password reset email
            await _loginManager.SendPasswordResetEmail(Email, resetToken);

            // Display a success message
            ViewData["SuccessMessage"] = "Na váš email byla odeslána žádost o resetování hesla.";

            return Page();
        }
    }
}
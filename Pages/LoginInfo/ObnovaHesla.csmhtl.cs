using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using VstupenkyWeb.Models; // Assuming your models are in this namespace

namespace VstupenkyWeb.Pages.LoginInfo
{
    public class ObnovaHeslaModel : PageModel
    {
        private readonly LoginManager _loginManager; // Inject your LoginManager

        public ObnovaHeslaModel(LoginManager loginManager)
        {
            _loginManager = loginManager;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Token { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Nové heslo je povinné.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Potvrzení hesla je povinné.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Hesla se neshodují.")]
        public string ConfirmPassword { get; set; }

        public IActionResult OnGet(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Index");
            }

            Email = email;
            Token = token;

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Verify the reset token
            if (!_loginManager.VerifyPasswordResetToken(Email, Token))
            {
                ModelState.AddModelError("", "Neplatný resetovací token.");
                return Page();
            }

            // Reset the password
            _loginManager.ResetPassword(Email, NewPassword);

            // Display a success message
            ViewData["SuccessMessage"] = "Heslo bylo úspěšně resetováno.";

            return RedirectToPage("/Index");
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using VstupenkyWeb.Models;
using System.Security.Claims;

namespace VstupenkyWeb.Pages.UdajeZmena
{
    public class HesloZmenaModel : PageModel
    {
        private readonly LoginManager _loginManager;
        private readonly IPasswordHasher<IdentityUser> _passwordHasher;

        public HesloZmenaModel(LoginManager loginManager, IPasswordHasher<IdentityUser> passwordHasher)
        {
            _loginManager = loginManager;
            _passwordHasher = passwordHasher;
        }

        [BindProperty]
        public string OldPassword { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public string PasswordChangeErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = GetCurrentUserId();
            var user = _loginManager.GetUserById(userId);

            if (user == null)
            {
                PasswordChangeErrorMessage = "Uživatel nenalezen.";
                return Page();
            }

            if (!_loginManager.VerifyPassword(OldPassword, user.heslo))
            {
                PasswordChangeErrorMessage = "Stávající heslo je neplatné.";
                return Page();
            }

            if (NewPassword != ConfirmPassword)
            {
                PasswordChangeErrorMessage = "Nová hesla se neshodují.";
                return Page();
            }

            if (!string.IsNullOrEmpty(NewPassword))
            {
                _loginManager.UpdateUserPassword(userId, NewPassword, _passwordHasher);
            }

            return RedirectToPage("/EditProfilu");
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return -1;
        }
    }
}
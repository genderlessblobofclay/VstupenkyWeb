using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VstupenkyWeb.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;

namespace VstupenkyWeb.Pages.UdajeZmena
{
    public class MailZmenaModel : PageModel
    {
        private readonly LoginManager _loginManager;

        public MailZmenaModel(LoginManager loginManager)
        {
            _loginManager = loginManager;
        }

        [BindProperty]
        public string NewEmail { get; set; }

        public IActionResult OnGet()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid)
    {
        return Page();
    }

    var userId = GetCurrentUserId();
    if (!string.IsNullOrEmpty(NewEmail))
    {
        _loginManager.UpdateUserEmail(userId, NewEmail);

        // Get the existing claims
        var claims = User.Claims.ToList();

        // Find the existing email claim and replace it
        var emailClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
        if (emailClaim != null)
        {
            claims.Remove(emailClaim);
        }
        claims.Add(new Claim(ClaimTypes.Email, NewEmail));

        // Create a new ClaimsIdentity with the updated claims
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        // Sign in with the updated principal
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
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
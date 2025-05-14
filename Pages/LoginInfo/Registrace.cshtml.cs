using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VstupenkyWeb.Models; // Add this to use the Role enum
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace VstupenkyWeb.Pages.LoginInfo
{
    public class RegistraceModel : PageModel
    {
        private readonly LoginManager _loginManager;

        public RegistraceModel(LoginManager loginManager)
        {
            _loginManager = loginManager;
        }

        [BindProperty]
        public NovyUzivatel NovyUzivatel { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Check if the login already exists
                if (_loginManager.LoginExists(NovyUzivatel.login))
                {
                    ModelState.AddModelError("NovyUzivatel.login", "Uživatel s tímto loginem již existuje.");
                    return Page();
                }

                // Check if the email already exists
                if (_loginManager.EmailExists(NovyUzivatel.email))
                {
                    ModelState.AddModelError("NovyUzivatel.email", "Uživatel s tímto emailem již existuje.");
                    return Page();
                }
                if (User.IsInRole("Admin"))
                    {

                    }
                else
                    {
                        NovyUzivatel.prava = VstupenkyWeb.Models.Role.Navstevnik;
                    }
                // Use the LoginManager to add the user
                _loginManager.PridatUzivatele(
                    NovyUzivatel.jmeno,
                    NovyUzivatel.prijmeni,
                    (int)NovyUzivatel.prava, // Pass the Role enum value as an integer
                    NovyUzivatel.login,
                    NovyUzivatel.heslo, // Pass the plain text password
                    NovyUzivatel.email
                );
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError("", "Došlo k chybě při registraci uživatele.");
                Console.WriteLine($"Error during registration: {ex}"); // Detailed logging
                return Page();
            }

            return RedirectToPage("/Index"); // Redirect to login page after successful registration
        }
    }

    public class NovyUzivatel
    {
        [Required(ErrorMessage = "Login je povinný.")]
        public string login { get; set; }

        [Required(ErrorMessage = "Heslo je povinné.")]
        [DataType(DataType.Password)]
        public string heslo { get; set; }

        [Required(ErrorMessage = "Email je povinný.")]
        [EmailAddress(ErrorMessage = "Neplatný formát emailu.")]
        public string email { get; set; }

        [Required(ErrorMessage = "Jméno je povinné.")]
        public string jmeno { get; set; }

        [Required(ErrorMessage = "Příjmení je povinné.")]
        public string prijmeni { get; set; }
       public VstupenkyWeb.Models.Role prava { get; set; }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VstupenkyWeb.Models; // Add this to use the Role enum
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace VstupenkyWeb.Pages.LoginInfo
{
    [AllowAnonymous]
    public class RegistraceModel : PageModel
    {
        private readonly LoginManager _loginManager;
        private readonly IWebHostEnvironment _environment;

        public RegistraceModel(LoginManager loginManager, IWebHostEnvironment environment)
        {
            _loginManager = loginManager;
            _environment = environment;
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

                // Handle profile icon upload
                string profileIconPath = null;
                if (NovyUzivatel.profileIcon != null)
                {
                    // Save the profile icon to the server
                    var fileName = Path.GetFileName(NovyUzivatel.profileIcon.FileName);
                    var filePath = Path.Combine(_environment.WebRootPath, "images", fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await NovyUzivatel.profileIcon.CopyToAsync(fileStream);
                    }

                    // Save the file path
                    profileIconPath = "/images/" + fileName;
                }

                if (User.IsInRole("Admin"))
                {

                }

                // Use the LoginManager to add the user
                _loginManager.PridatUzivatele(
                NovyUzivatel.jmeno,
                NovyUzivatel.prijmeni,
                (int)NovyUzivatel.prava, // Pass the Role enum value as an integer
                NovyUzivatel.login,
                NovyUzivatel.heslo, // Pass the plain text password
                NovyUzivatel.email,
                profileIconPath // Pass the profile icon file path
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

    [Display(Name = "Profilový obrázek")]
   public IFormFile profileIcon { get; set; }
}

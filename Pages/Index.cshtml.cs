using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using VstupenkyWeb.Models; // Import the Models namespace
using Microsoft.AspNetCore.Identity;

namespace VstupenkyWeb.Pages
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly LoginManager _loginManager; // Add LoginManager

        [BindProperty]
        [Required(ErrorMessage = "Login je povinný.")]
        public string login { get; set; } = ""; // Initialize

        [BindProperty]
        [Required(ErrorMessage = "Heslo je povinné.")]
        [DataType(DataType.Password)]
        public string heslo { get; set; } = ""; // Initialize

        public string ErrorMessage { get; set; } = ""; // Initialize

        public IndexModel(IConfiguration configuration, LoginManager loginManager) // Inject LoginManager
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("VstupenkyDB") ?? ""; // Null-conditional
            _loginManager = loginManager; // Initialize LoginManager
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Ověření uživatele v databázi
            var user = AuthenticateUser(login, heslo);
            if (user != null)
            {
                // Vytvoření claims pro autentizaci
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Uzivatele_ID.ToString()), // Přidán ID uživatele
                    new Claim(ClaimTypes.Name, user.login),
                    new Claim("Jmeno", user.jmeno), // Příklad přidání jména
                    new Claim("Prijmeni", user.prijmeni), // Příklad přidání příjmení
                    new Claim(ClaimTypes.Email, user.email), // Příklad přidání emailu
                    new Claim(ClaimTypes.Role, user.prava.ToString()) // Add the role claim as integer
                    // Můžete přidat další claimy podle potřeby
                };

                // Vytvoření identity a principalu
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false, // Nastavte na true, pokud chcete, aby byl uživatel přihlášený i po zavření prohlížeče
                };
                var principal = new ClaimsPrincipal(claimsIdentity);

                // Přihlášení uživatele pomocí Cookie Authentication
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

               
                return RedirectToPage("/Vstupenky/Rezervace");
            }
            else
            {
                ErrorMessage = "Neplatné přihlašovací údaje.";
                return Page();
            }
        }

        private User AuthenticateUser(string login, string heslo)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    string sql = "SELECT Uzivatel_ID, jmeno, prijmeni, email, login, heslo, prava FROM [devextlunch].[Uzivatel] WHERE login = @login"; // Include 'prava'
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@login", login);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var storedHashedPassword = reader["heslo"].ToString();
                                var user = new User
                                {
                                    Uzivatele_ID = (int)reader["Uzivatel_ID"],
                                    jmeno = reader["jmeno"] != DBNull.Value ? reader["jmeno"].ToString() : "", // Handle potential null values
                                    prijmeni = reader["prijmeni"] != DBNull.Value ? reader["prijmeni"].ToString() : "", // Handle potential null values
                                    email = reader["email"] != DBNull.Value ? reader["email"].ToString() : "", // Handle potential null values
                                    login = reader["login"].ToString(),
                                    prava = (Role)(int)reader["prava"] // Retrieve and cast 'prava'
                                };

                                // Verify the password using the LoginManager
                                var passwordHasher = new PasswordHasher<object>();
                                var result = passwordHasher.VerifyHashedPassword(null, storedHashedPassword, heslo);
                                if (result == PasswordVerificationResult.Success)
                                {
                                    return user;
                                }
                            }
                            return null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Chyba při připojení k databázi.";
                    Console.WriteLine($"Chyba DB: {ex.Message}");
                    return null;
                }
            }
        }       
    }
}
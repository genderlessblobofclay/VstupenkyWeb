using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using VstupenkyWeb.Models; // Assuming your models are in this namespace
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace VstupenkyWeb.Pages
{
    public class EditProfiluModel : PageModel
    {
        private readonly LoginManager _loginManager; // Inject your LoginManager
        private readonly IPasswordHasher<IdentityUser> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public string ErrorMessage { get; set; } = ""; // Initialize it here
        public string PasswordChangeErrorMessage { get; set; } = ""; // Add this line

        public EditProfiluModel(LoginManager loginManager, IPasswordHasher<IdentityUser> passwordHasher, IConfiguration configuration)
        {
            _loginManager = loginManager;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("VstupenkyDB");
        }

        public string Jmeno { get; set; }
        public string Prijmeni { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string ProfileIconPath { get; set; }
      
        [BindProperty]
        [EmailAddress]
        public string NewEmail { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [BindProperty]
        public IFormFile ProfileIcon { get; set; }

        public IActionResult OnGet()
        {
            // Check if the user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Index"); // Redirect to login page if not authenticated
            }

            // Get the current user's ID
            var userId = GetCurrentUserId();

            // Load user data
            LoadUserData(userId);

            return Page();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
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

            // Verify the old password
            if (!_loginManager.VerifyPassword(OldPassword, user.heslo))
            {
                PasswordChangeErrorMessage = "Stávající heslo je neplatné.";
                return Page();
            }

            // Check if the new passwords match
            if (NewPassword != ConfirmPassword)
            {
                PasswordChangeErrorMessage = "Nová hesla se neshodují.";
                return Page();
            }

            if (!string.IsNullOrEmpty(NewPassword))
            {
                _loginManager.UpdateUserPassword(userId, NewPassword, _passwordHasher);
            }

            LoadUserData(GetCurrentUserId());
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = GetCurrentUserId();
            if (!string.IsNullOrEmpty(NewEmail))
            {
                _loginManager.UpdateUserEmail(userId, NewEmail);

                // Update the email claim
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, NewEmail),
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }

            LoadUserData(GetCurrentUserId());
            return Page();
        }

        public async Task<IActionResult> OnPostChangeIconAsync()
        {
            if (ProfileIcon != null)
            {
                var userId = GetCurrentUserId();
                var fileName = Path.GetFileName(ProfileIcon.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileIcon.CopyToAsync(fileStream);
                }

                // Save the file path to the database
                _loginManager.UpdateUserProfileIcon(userId, "/images/" + fileName);
            }

            LoadUserData(GetCurrentUserId());
            return Page();
        }

        private void LoadUserData(int userId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = "SELECT jmeno, prijmeni, login, email, ikona, heslo FROM [devextlunch].[Uzivatel] WHERE Uzivatel_ID = @ID";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ID", userId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Jmeno = reader["jmeno"].ToString();
                                Prijmeni = reader["prijmeni"].ToString();
                                Login = reader["login"].ToString();
                                Email = reader["email"].ToString();
                                ProfileIconPath = reader["ikona"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error loading user data: {ex}");
                ErrorMessage = "Error loading user data.";
            }
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
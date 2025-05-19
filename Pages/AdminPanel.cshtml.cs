using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VstupenkyWeb.Models;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;

namespace VstupenkyWeb.Pages
{
    [Authorize(Roles = "Admin")]
    public class AdminPanelModel : PageModel
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly LoginManager _loginManager; // Add this line
        private readonly VstupenkyManager _vstupenkyManager;

        public string ErrorMessage { get; set; }
        public List<User> Users { get; set; } = new List<User>();

        public AdminPanelModel(IConfiguration configuration, LoginManager loginManager, VstupenkyManager vstupenkyManager) // Modify this line
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("VstupenkyDB");
            _loginManager = loginManager; // Add this line
            _vstupenkyManager = vstupenkyManager;
        }

        public void OnGet()
        {
            Users = GetUsers();
        }

        public IActionResult OnPostDelete(int id)
        {
            try
            {
                // Delete tickets associated with the user
                _vstupenkyManager.OdstranitVstupenkyProUzivatele(id);

                // Delete the user
                DeleteUser(id);

                Users = GetUsers(); // Refresh the user list after deletion
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error deleting user.";
                Console.WriteLine(ex.ToString());
                return Page();
            }
        }
        public async Task<IActionResult> OnPostUpdateRole(int id, int newRole)
        {
            try
            {
                UpdateUserRole(id, (Role)newRole);
                // Invalidate the user's cookie
                await InvalidateUserCookie(id);
                Users = GetUsers(); // Refresh the user list after update
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error updating user role.";
                Console.WriteLine(ex.ToString());
                return Page();
            }
        }

        private List<User> GetUsers()
        {
            List<User> users = new List<User>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = "SELECT Uzivatel_ID, jmeno, prijmeni, email, login, heslo, prava FROM [devextlunch].[Uzivatel]";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                User user = new User
                                {
                                    Uzivatele_ID = (int)reader["Uzivatel_ID"],
                                    login = reader["login"].ToString(),
                                    jmeno = reader["jmeno"].ToString(),
                                    prijmeni = reader["prijmeni"].ToString(),
                                    email = reader["email"].ToString(),
                                    prava = (Role)(int)reader["prava"]
                                };
                                users.Add(user);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error retrieving users.";
                Console.WriteLine(ex.ToString());
            }
            return users;
        }

        private void DeleteUser(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = "DELETE FROM [devextlunch].[Uzivatel] WHERE Uzivatel_ID = @ID";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ID", id);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error deleting user.";
                Console.WriteLine(ex.ToString());
            }
        }

        private void UpdateUserRole(int id, Role newRole)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = "UPDATE [devextlunch].[Uzivatel] SET prava = @NewRole WHERE Uzivatel_ID = @ID";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ID", id);
                        command.Parameters.AddWithValue("@NewRole", (int)newRole);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error updating user role.";
                Console.WriteLine(ex.ToString());
            }
        }

       private async Task InvalidateUserCookie(int userId)
        {
            // Find the user
            var user = _loginManager.GetUserById(userId);

            // If user is found
            if (user != null)
            {
                // Clear existing cookie
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }
    }
}

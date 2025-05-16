using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VstupenkyWeb.Models;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;

namespace VstupenkyWeb.Pages
{
    [Authorize(Roles = "Admin")]
    public class AdminPanelModel : PageModel
    {
        private readonly string _connectionString;
        public string ErrorMessage { get; set; }
        public List<User> Users { get; set; } = new List<User>();

        public AdminPanelModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("VstupenkyDB");
        }

        public void OnGet()
        {
            Users = GetUsers();
        }

        public IActionResult OnPostDelete(int id)
        {
            try
            {
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
    }
}
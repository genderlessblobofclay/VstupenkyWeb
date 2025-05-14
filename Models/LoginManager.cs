using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Linq; // Pro použití Take
using Microsoft.AspNetCore.Identity;

namespace VstupenkyWeb.Models
{
    public class LoginManager
    {
        private readonly string _connectionString;
        private readonly IPasswordHasher<IdentityUser> _passwordHasher;

        public LoginManager(IConfiguration configuration, IPasswordHasher<IdentityUser> passwordHasher)
        {
            _connectionString = configuration.GetConnectionString("VstupenkyDB") ?? "";
            _passwordHasher = passwordHasher;
        }

        private static string TabulkaUsers = "[devextlunch].[devextlunch].[Uzivatel]"; // Název vaší tabulky s uživateli

        public void PridatUzivatele(string jmeno, string prijmeni, int prava, string login, string heslo, string email)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Hash the password using ASP.NET Core Identity's PasswordHasher
                    var user = new IdentityUser(); // We only need this to hash the password.  We don't save it.
                    string hashedPassword = _passwordHasher.HashPassword(user, heslo);

                    // Truncate the hashed password to 50 characters
                    if (hashedPassword.Length > 50)
                    {
                        hashedPassword = hashedPassword.Substring(0, 50);
                    }

                    string sql = $"INSERT INTO {TabulkaUsers} (jmeno, prijmeni, prava, login, heslo, email) VALUES (@jmeno, @prijmeni, @prava, @login, @heslo, @email)";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@jmeno", jmeno);
                        command.Parameters.AddWithValue("@prijmeni", prijmeni);
                        command.Parameters.AddWithValue("@prava", prava);
                        command.Parameters.AddWithValue("@login", login);
                        command.Parameters.AddWithValue("@heslo", hashedPassword); // Store the hashed password
                        command.Parameters.AddWithValue("@email", email);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Uživatel s loginem '{login}' byl úspěšně registrován.");
                        }
                        else
                        {
                            Console.WriteLine($"Nepodařilo se registrovat uživatele s loginem '{login}'.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při registraci uživatele s loginem '{login}': {ex.Message}");
                // Zvažte logování chyby ve webovém prostředí
            }
        }

        public bool LoginExists(string login)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = $"SELECT COUNT(*) FROM {TabulkaUsers} WHERE login = @login";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    return (int)command.ExecuteScalar() > 0;
                }
            }
        }

        public bool EmailExists(string email)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = $"SELECT COUNT(*) FROM {TabulkaUsers} WHERE email = @email";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    return (int)command.ExecuteScalar() > 0;
                }
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var user = new IdentityUser(); // We only need this to hash the password. We don't save it.
            var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, password);
            return result == PasswordVerificationResult.Success;
        }

        // Můžete přidat další metody pro správu uživatelů, např. ověření, načtení atd.
    }
}
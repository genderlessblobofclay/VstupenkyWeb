// filepath: c:\Users\brigadnik1\Desktop\Praxe\ConsoleApp1\ConsoleApp1\VstupenkyWeb\Models\LoginManager.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Linq; // Pro použití Take
using Microsoft.AspNetCore.Identity;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace VstupenkyWeb.Models
{
    public class LoginManager
    {
        private readonly string _connectionString;
        private readonly IPasswordHasher<IdentityUser> _passwordHasher;
        private readonly IConfiguration _configuration;

        public LoginManager(IConfiguration configuration, IPasswordHasher<IdentityUser> passwordHasher)
        {
            _connectionString = configuration.GetConnectionString("VstupenkyDB") ?? "";
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        private async Task SendEmail(string to, string subject, string body)
        {
            var apiKey = _configuration["SendGrid:ApiKey"]; // Access API key from configuration
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("ppatrik1993@gmail.com", "Akce Ostrava"); // Replace with your email address
            var toEmail = new EmailAddress(to);
            var plainTextContent = System.Net.WebUtility.HtmlEncode(body); // Sanitize HTML for plain text
            var htmlContent = body;
            var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, plainTextContent, htmlContent);
            try
            {
                var response = await client.SendEmailAsync(msg);
                Console.WriteLine($"Email sent to {to}. Status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex}");
            }
        }

        private static string TabulkaUsers = "[devextlunch].[devextlunch].[Uzivatel]"; // Název vaší tabulky s uživateli

        public void PridatUzivatele(string jmeno, string prijmeni, int prava, string login, string heslo, string email, string profileIconPath)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = "INSERT INTO [devextlunch].[Uzivatel] (jmeno, prijmeni, prava, login, heslo, email, ikona) VALUES (@jmeno, @prijmeni, @prava, @login, @heslo, @email, @ikona)";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@jmeno", jmeno);
                        command.Parameters.AddWithValue("@prijmeni", prijmeni);
                        command.Parameters.AddWithValue("@prava", prava);
                        command.Parameters.AddWithValue("@login", login);

                        // Hash the password here
                        var passwordHasher = new PasswordHasher<object>();
                        string hashedPassword = passwordHasher.HashPassword(null, heslo);

                        command.Parameters.AddWithValue("@heslo", hashedPassword);
                        command.Parameters.AddWithValue("@email", email);
                        command.Parameters.AddWithValue("@ikona", profileIconPath ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error adding user: {ex}");
                throw; // Re-throw the exception to indicate failure
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



        public string GeneratePasswordResetToken(string email)
        {
            // Generate a unique token
            return Guid.NewGuid().ToString();
        }

        public async Task SendPasswordResetEmail(string email, string resetToken)
        {
            // Create the password reset link
            string resetLink = $"https://yourwebsite.com/LoginInfo/ResetPassword?email={email}&token={resetToken}";

            // Create the email message
            string subject = "Žádost o resetování hesla";
            string body = $"Pro resetování hesla klikněte na následující odkaz: <a href=\"{resetLink}\">Resetovat heslo</a>";

            // Send the email (you'll need to implement your email sending logic here)
            await SendEmail(email, subject, body);
        }
        public bool VerifyPasswordResetToken(string email, string token)
        {
            // Verify that the token is valid for the given email
            // This is just a placeholder
            return true;
        }

        public void ResetPassword(string email, string newPassword)
        {
            // Reset the user's password in the database
            // This is just a placeholder
            Console.WriteLine($"Resetting password for email: {email}, new password: {newPassword}");
        }

        public void UpdateUserRole(int userId, int roleId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = "UPDATE [devextlunch].[Uzivatel] SET Prava = @roleId WHERE Uzivatel_ID = @userId";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@roleId", roleId);
                        command.Parameters.AddWithValue("@userId", userId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error updating user role: {ex}");
                throw; // Re-throw the exception to indicate failure
            }
        }

        public void UpdateUserEmail(int userId, string newEmail)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = "UPDATE [devextlunch].[Uzivatel] SET email = @newEmail WHERE Uzivatel_ID = @userId";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@newEmail", newEmail);
                        command.Parameters.AddWithValue("@userId", userId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error updating user email: {ex}");
                throw; // Re-throw the exception to indicate failure
            }
        }

        public void UpdateUserPassword(int userId, string newPassword, IPasswordHasher<IdentityUser> passwordHasher)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Hash the new password
                    var user = new IdentityUser();
                    string hashedPassword = passwordHasher.HashPassword(user, newPassword);

                    string sql = "UPDATE [devextlunch].[Uzivatel] SET heslo = @Heslo WHERE Uzivatel_ID = @ID";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ID", userId);
                        command.Parameters.AddWithValue("@Heslo", hashedPassword);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Heslo uživatele s ID '{userId}' bylo úspěšně změněno.");
                        }
                        else
                        {
                            Console.WriteLine($"Nepodařilo se změnit heslo uživatele s ID '{userId}'.");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Log the error
                Console.WriteLine($"Chyba při změně hesla uživatele: {ex.Message}");
                throw; // Re-throw the exception
            }
        }

        public void UpdateUserProfileIcon(int userId, string profileIconPath)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = "UPDATE [devextlunch].[Uzivatel] SET ikona = @profileIconPath WHERE Uzivatel_ID = @userId";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@profileIconPath", profileIconPath);
                        command.Parameters.AddWithValue("@userId", userId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error updating user profile icon: {ex}");
                throw; // Re-throw the exception to indicate failure
            }
        }

        public User GetUserById(int userId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = "SELECT Uzivatel_ID, jmeno, prijmeni, email, login, heslo, prava FROM [devextlunch].[Uzivatel] WHERE Uzivatel_ID = @ID";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ID", userId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    Uzivatele_ID = (int)reader["Uzivatel_ID"],
                                    jmeno = reader["jmeno"].ToString(),
                                    prijmeni = reader["prijmeni"].ToString(),
                                    email = reader["email"].ToString(),
                                    login = reader["login"].ToString(),
                                    heslo = reader["heslo"].ToString(), // Retrieve the hashed password
                                    prava = (Role)(int)reader["prava"]
                                };
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by ID: {ex}");
                return null;
            }
        }
        
        
    }

    
}
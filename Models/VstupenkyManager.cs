using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace VstupenkyWeb.Models
{
    public class Vstupenka
    {
        public int Id { get; set; }
        public string Jmeno { get; set; } = "";
        public int Pocet { get; set; }
        public int Uzivatel_ID { get; set; }
    }

    public class VstupenkyManager
    {
        private readonly string _connectionString;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VstupenkyManager(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _connectionString = configuration.GetConnectionString("VstupenkyDB") ?? "";
            _httpContextAccessor = httpContextAccessor;
        }

        private static string TabulkaVstupenky = "[devextlunch].[Table_1]";

        public List<Vstupenka> VypisVstupenky()
        {
            var vstupenky = new List<Vstupenka>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = $"SELECT vstupenka_id, jmeno, pocet, Uzivatel_ID FROM {TabulkaVstupenky}";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Vstupenka vstupenka = new Vstupenka
                                {
                                    Id = (int)reader["vstupenka_id"],
                                    Jmeno = reader["jmeno"] == DBNull.Value ? "" : reader["jmeno"].ToString(),
                                    Pocet = (int)reader["pocet"],
                                    Uzivatel_ID = reader.IsDBNull(reader.GetOrdinal("Uzivatel_ID")) ? 0 : (int)reader["Uzivatel_ID"]
                                };
                                Console.WriteLine($"Ticket ID: {vstupenka.Id}, Name: {vstupenka.Jmeno}, Count: {vstupenka.Pocet}, User ID: {vstupenka.Uzivatel_ID}"); // Add this line
                                vstupenky.Add(vstupenka);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při načítání dat: {ex.Message}");
            }
            return vstupenky;
        }

        public void PridatVstupenku(string jmeno, int pocet)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Get the current user's ID
                    var userId = GetCurrentUserId();

                    string selectMaxIdSql = $"SELECT ISNULL(MAX(vstupenka_id), 0) + 1 FROM {TabulkaVstupenky}";
                    using (SqlCommand selectMaxIdCommand = new SqlCommand(selectMaxIdSql, connection))
                    {
                        int noveId = (int)selectMaxIdCommand.ExecuteScalar();

                        string sql = $"INSERT INTO {TabulkaVstupenky} (vstupenka_id, jmeno, pocet, Uzivatel_ID) VALUES (@id, @jmeno, @pocet, @userId)";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@id", noveId);
                            command.Parameters.AddWithValue("@jmeno", jmeno);
                            command.Parameters.AddWithValue("@pocet", pocet);
                            command.Parameters.AddWithValue("@userId", userId); // Store the user's ID

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                Console.WriteLine("Nový záznam byl úspěšně přidán.");
                            }
                            else
                            {
                                Console.WriteLine("Nepodařilo se přidat nový záznam.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při přidávání dat: {ex.Message}");
            }
        }

        public void OdstranitVstupenku(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Get the current user's ID
                    var userId = GetCurrentUserId();

                    // Check if the user is authorized to delete the ticket
                    if (!IsAuthorizedToDelete(id, userId))
                    {
                        // Throw an exception or handle the unauthorized access as needed
                        throw new UnauthorizedAccessException("You are not authorized to delete this ticket.");
                    }

                    string sql = $"DELETE FROM {TabulkaVstupenky} WHERE Vstupenka_ID = @id";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Vstupenka s ID {id} byla úspěšně odstraněna.");
                        }
                        else
                        {
                            Console.WriteLine($"Vstupenka s ID {id} nebyla nalezena.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při odstraňování vstupenky s ID {id}: {ex.Message}");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            // Handle the case where the user is not authenticated or the ID claim is not found
            return -1;
        }

        private bool IsAuthorizedToDelete(int ticketId, int userId)
        {
            // Admins are always authorized
            if (_httpContextAccessor.HttpContext.User.IsInRole("Admin"))
            {
                return true;
            }

            // Check if the user is the owner of the ticket
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = "SELECT Uzivatel_ID FROM [devextlunch].[Table_1] WHERE Vstupenka_ID = @Id";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", ticketId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int ticketUserId = (int)reader["Uzivatel_ID"];
                                return ticketUserId == userId;
                            }
                            return false; // Ticket not found
                        }
                    }
                }
            }
            catch
            {
                return false; // Error occurred, deny access
            }
        }


        public void OdstranitVstupenkyProUzivatele(int userId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = $"DELETE FROM {TabulkaVstupenky} WHERE Uzivatel_ID = @userId";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        int rowsAffected = command.ExecuteNonQuery();
                        Console.WriteLine($"{rowsAffected} tickets deleted for user ID {userId}.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting tickets for user ID {userId}: {ex.Message}");
            }
        }
    }
}
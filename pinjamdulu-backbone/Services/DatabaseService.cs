using Npgsql;
using pinjamdulu_backbone.Helpers;
using pinjamdulu_backbone.Models;
using System.Text;
using System.Security.Cryptography;

namespace pinjamdulu_backbone.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = ConfigurationHelper.GetConnectionString();
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public async Task<User> AuthenticateUser(string email, string password)
        {
            string hashedPassword = HashPassword(password);

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var sql = @"SELECT user_id, full_name, username, email, birth_date, address, city, contact, profile_picture 
                           FROM public.""User"" 
                           WHERE email = @email AND password = @password";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("email", email);
                    cmd.Parameters.AddWithValue("password", hashedPassword);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                UserId = reader.GetGuid(0),
                                FullName = reader.GetString(1),
                                Username = reader.GetString(2),
                                Email = reader.GetString(3),
                                BirthDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                                Address = reader.IsDBNull(5) ? null : reader.GetString(5),
                                City = reader.IsDBNull(6) ? null : reader.GetString(6),
                                Contact = reader.IsDBNull(7) ? null : reader.GetString(7),
                                ProfilePicture = reader.IsDBNull(8) ? null : (byte[])reader[8]
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<bool> CheckEmailExists(string email)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var sql = @"SELECT COUNT(*) FROM public.""User"" WHERE email = @email";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("email", email);
                    var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }

        public async Task<bool> CreateUser(User user, string password)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"
                        INSERT INTO public.""User"" (user_id, full_name, username, email, password, birth_date, address, city, contact, profile_picture)
                        VALUES (@userId, @fullName, @username, @email, @password, @birthDate, @address, @city, @contact, @profilePicture)";

                    cmd.Parameters.AddWithValue("userId", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("fullName", user.FullName);
                    cmd.Parameters.AddWithValue("username", user.Username);
                    cmd.Parameters.AddWithValue("email", user.Email);
                    cmd.Parameters.AddWithValue("password", HashPassword(password));
                    cmd.Parameters.AddWithValue("birthDate", (object)user.BirthDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("address", (object)user.Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("city", (object)user.City ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("contact", (object)user.Contact ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("profilePicture", user.ProfilePicture);

                    try
                    {
                        await cmd.ExecuteNonQueryAsync();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
        }
    }
}
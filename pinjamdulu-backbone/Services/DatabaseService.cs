﻿using Npgsql;
using pinjamdulu_backbone.Helpers;
using pinjamdulu_backbone.Models;
using System.Text;
using System.Security.Cryptography;

namespace pinjamdulu_backbone.Services
{
    
    public class DatabaseService
    {
        //--------------------------- AUTHENTICATION SERVICES ---------------------------//

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

        //--------------------------- LISTING GADGET SERVICES ---------------------------//
        public async Task<List<Gadget>> GetUserGadgets(Guid userId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var gadgets = new List<Gadget>();

                var sql = @"
                            SELECT g.*, 
                                   gi.image[1] as first_image,
                                   COUNT(DISTINCT b.booking_id) as times_rented,
                                   current_booking.borrower_username,
                                   current_booking.rental_start_date,
                                   current_booking.rental_end_date
                            FROM public.""Gadget"" g
                            LEFT JOIN public.""GadgetImages"" gi ON g.gadget_id = gi.gadget_id
                            LEFT JOIN public.""Booking"" b ON g.gadget_id = b.gadget_id
                            LEFT JOIN (
                                SELECT b.gadget_id, 
                                       u.username as borrower_username,
                                       b.rental_start_date,
                                       b.rental_end_date
                                FROM public.""Booking"" b
                                JOIN public.""User"" u ON b.borrower_id = u.user_id
                                WHERE CURRENT_DATE BETWEEN b.rental_start_date AND b.rental_end_date
                            ) current_booking ON g.gadget_id = current_booking.gadget_id
                            WHERE g.owner_id = @userId
                            GROUP BY g.gadget_id, gi.image[1], 
                                     current_booking.borrower_username,
                                     current_booking.rental_start_date,
                                     current_booking.rental_end_date";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("userId", userId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var gadget = new Gadget
                            {
                                GadgetId = reader.GetGuid(reader.GetOrdinal("gadget_id")),
                                OwnerId = reader.GetGuid(reader.GetOrdinal("owner_id")),
                                Title = reader.GetString(reader.GetOrdinal("title")),
                                Description = reader.GetString(reader.GetOrdinal("description")),
                                Category = reader.GetString(reader.GetOrdinal("category")),
                                Brand = reader.GetString(reader.GetOrdinal("brand")),
                                ConditionMetric = reader.GetInt32(reader.GetOrdinal("condition_metric")),
                                GadgetRating = reader.GetFloat(reader.GetOrdinal("gadget_rating")),
                                RentalPrice = reader.GetDecimal(reader.GetOrdinal("rental_price")),
                                Availability = reader.GetBoolean(reader.GetOrdinal("availability")),
                                AvailabilityDate = reader.IsDBNull(reader.GetOrdinal("availability_date")) ? null : reader.GetDateTime(reader.GetOrdinal("availability_date")),
                                Images = reader.IsDBNull(reader.GetOrdinal("first_image")) ? null : new[] { (byte[])reader["first_image"] },
                                TimesRented = reader.GetInt32(reader.GetOrdinal("times_rented")),
                                CurrentRenterUsername = reader.IsDBNull(reader.GetOrdinal("borrower_username")) ? null : reader.GetString(reader.GetOrdinal("borrower_username")),
                                CurrentRentalStart = reader.IsDBNull(reader.GetOrdinal("rental_start_date")) ? null : reader.GetDateTime(reader.GetOrdinal("rental_start_date")),
                                CurrentRentalEnd = reader.IsDBNull(reader.GetOrdinal("rental_end_date")) ? null : reader.GetDateTime(reader.GetOrdinal("rental_end_date"))
                            };
                            gadgets.Add(gadget);
                        }
                    }
                }
                return gadgets;
            }
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> AddGadget(Gadget gadget)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // Insert gadget
                        var gadgetSql = @"
                                        INSERT INTO public.""Gadget"" (gadget_id, owner_id, title, description, 
                                        category, brand, condition_metric, gadget_rating, rental_price, 
                                        availability, availability_date)
                                        VALUES (@gadgetId, @ownerId, @title, @description, @category, 
                                        @brand, @conditionMetric, @gadgetRating, @rentalPrice, 
                                        @availability, @availabilityDate)";

                        using (var cmd = new NpgsqlCommand(gadgetSql, conn, transaction))
                        {
                            var gadgetId = Guid.NewGuid();
                            cmd.Parameters.AddWithValue("gadgetId", gadgetId);
                            cmd.Parameters.AddWithValue("ownerId", gadget.OwnerId);
                            cmd.Parameters.AddWithValue("title", gadget.Title);
                            cmd.Parameters.AddWithValue("description", gadget.Description);
                            cmd.Parameters.AddWithValue("category", gadget.Category);
                            cmd.Parameters.AddWithValue("brand", gadget.Brand);
                            cmd.Parameters.AddWithValue("conditionMetric", gadget.ConditionMetric);
                            cmd.Parameters.AddWithValue("gadgetRating", 0); // New gadget starts with 0 rating
                            cmd.Parameters.AddWithValue("rentalPrice", gadget.RentalPrice);
                            cmd.Parameters.AddWithValue("availability", true);
                            cmd.Parameters.AddWithValue("availabilityDate", (object)gadget.AvailabilityDate ?? DBNull.Value);

                            await cmd.ExecuteNonQueryAsync();

                            // Insert images
                            if (gadget.Images != null && gadget.Images.Length > 0)
                            {
                                var imagesSql = @"
                                                INSERT INTO public.""GadgetImages"" (gadget_image_id, gadget_id, image)
                                                VALUES (@imageId, @gadgetId, @image)";

                                using (var imgCmd = new NpgsqlCommand(imagesSql, conn, transaction))
                                {
                                    //foreach (var image in gadget.Images)
                                    //{
                                    //    imgCmd.Parameters.Clear();
                                    //    imgCmd.Parameters.AddWithValue("imageId", Guid.NewGuid());
                                    //    imgCmd.Parameters.AddWithValue("gadgetId", gadgetId);
                                    //    imgCmd.Parameters.AddWithValue("image", image);
                                    //    await imgCmd.ExecuteNonQueryAsync();
                                    //}

                                    imgCmd.Parameters.Clear();
                                    imgCmd.Parameters.AddWithValue("imageId", Guid.NewGuid());
                                    imgCmd.Parameters.AddWithValue("gadgetId", gadgetId);
                                    imgCmd.Parameters.AddWithValue("image", gadget.Images);
                                    await imgCmd.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        return (true, string.Empty); // Operation succeeded, no error message
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return (false, ex.Message); // Return error message
                    }
                }
            }
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> UpdateGadget(Gadget gadget)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        var sql = @"
                                    UPDATE public.""Gadget""
                                    SET title = @title,
                                        description = @description,
                                        category = @category,
                                        brand = @brand,
                                        condition_metric = @conditionMetric,
                                        rental_price = @rentalPrice,
                                        availability_date = @availabilityDate
                                    WHERE gadget_id = @gadgetId";

                        using (var cmd = new NpgsqlCommand(sql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("gadgetId", gadget.GadgetId);
                            cmd.Parameters.AddWithValue("title", gadget.Title);
                            cmd.Parameters.AddWithValue("description", gadget.Description);
                            cmd.Parameters.AddWithValue("category", gadget.Category);
                            cmd.Parameters.AddWithValue("brand", gadget.Brand);
                            cmd.Parameters.AddWithValue("conditionMetric", gadget.ConditionMetric);
                            cmd.Parameters.AddWithValue("rentalPrice", gadget.RentalPrice);
                            cmd.Parameters.AddWithValue("availabilityDate", (object)gadget.AvailabilityDate ?? DBNull.Value);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        // Update images if new ones are provided
                        if (gadget.Images != null && gadget.Images.Length > 0)
                        {
                            // Delete existing images
                            var deleteImagesSql = @"DELETE FROM public.""GadgetImages"" WHERE gadget_id = @gadgetId";
                            using (var delCmd = new NpgsqlCommand(deleteImagesSql, conn, transaction))
                            {
                                delCmd.Parameters.AddWithValue("gadgetId", gadget.GadgetId);
                                await delCmd.ExecuteNonQueryAsync();
                            }

                            // Insert new images
                            var insertImagesSql = @"
                                                    INSERT INTO public.""GadgetImages"" (gadget_image_id, gadget_id, image)
                                                    VALUES (@imageId, @gadgetId, @image)";

                            using (var imgCmd = new NpgsqlCommand(insertImagesSql, conn, transaction))
                            {
                                //foreach (var image in gadget.Images)
                                //{
                                //    imgCmd.Parameters.Clear();
                                //    imgCmd.Parameters.AddWithValue("imageId", Guid.NewGuid());
                                //    imgCmd.Parameters.AddWithValue("gadgetId", gadget.GadgetId);
                                //    imgCmd.Parameters.AddWithValue("image", image);
                                //    await imgCmd.ExecuteNonQueryAsync();
                                //}
                                imgCmd.Parameters.Clear();
                                imgCmd.Parameters.AddWithValue("imageId", Guid.NewGuid());
                                imgCmd.Parameters.AddWithValue("gadgetId", gadget.GadgetId);
                                imgCmd.Parameters.AddWithValue("image", gadget.Images);
                                await imgCmd.ExecuteNonQueryAsync();
                            }
                        }

                        await transaction.CommitAsync();
                        return (true, string.Empty);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return (false, ex.Message); // Return error message
                    }
                }
            }
        }

    }
}
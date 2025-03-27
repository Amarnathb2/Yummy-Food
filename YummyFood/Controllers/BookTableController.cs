using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using YummyFood.Models;

namespace YummyFood.Controllers
{
    public class BookTableController : Controller
    {
        private readonly IConfiguration _configuration;

        public BookTableController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpPost]
        public async Task<IActionResult> BookTableAsync(BookTable booking)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            string userId = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Retrieve UserId from User table using Email
                    string getUserQuery = "SELECT UserId FROM [User] WHERE Email = @Email";
                    using (SqlCommand getUserCommand = new SqlCommand(getUserQuery, connection))
                    {
                        getUserCommand.Parameters.AddWithValue("@Email", booking.Email);
                        var result = await getUserCommand.ExecuteScalarAsync();

                        if (result != null)
                        {
                            userId = result.ToString();
                        }
                        else
                        {
                            return BadRequest("User not found with the provided email.");
                        }
                    }

                    // Insert booking details
                    string query = "INSERT INTO BookATable (Name, Email, Phone, Date, Time, People, UserId) VALUES (@Name, @Email, @Phone, @Date, @Time, @People, @UserId)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", booking.Name);
                        command.Parameters.AddWithValue("@Email", booking.Email);
                        command.Parameters.AddWithValue("@Phone", booking.Phone);
                        command.Parameters.AddWithValue("@Date", booking.Date);
                        command.Parameters.AddWithValue("@Time", booking.Time);
                        command.Parameters.AddWithValue("@People", booking.People);
                        command.Parameters.AddWithValue("@UserId", userId);

                        int insertResult = command.ExecuteNonQuery();
                        if (insertResult > 0)
                        {
                            return Redirect("/");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
            return View();
        }
    }
}

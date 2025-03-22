using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YummyFood.Models;
using System.Data.SqlClient;

namespace YummyFood.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IConfiguration _configuration;
        public RegisterController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet]
        [Route("/signup")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Registeration(string name, string email, string password, string phone, string gender)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string userId = GenerateUserId();

                    string query = "INSERT INTO User (UserId, UserName, Email, Password, Phone, Gender) VALUES (@UserId, @UserName, @Email, @Password, @Phone, @Gender)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@UserName", name);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password); // Consider hashing
                        command.Parameters.AddWithValue("@Phone", phone);
                        command.Parameters.AddWithValue("@Gender", gender);

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            return RedirectToAction("Success");
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

        private string GenerateUserId()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
        }
    }
}

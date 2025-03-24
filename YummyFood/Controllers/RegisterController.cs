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
        private readonly EmailService _emailService;

        public RegisterController(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
        }
        [HttpGet]
        [Route("/signup")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RegistrationAsync(User user)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string userId = GenerateUserId();
                    var username= user.FirstName +" "+user.LastName;
                    string query = "INSERT INTO [User] (UserId, UserName, Email, Password, Birthday, Gender, Phone) VALUES (@UserId, @UserName, @Email, @Password,@Birthday, @Gender, @Phone)";
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@UserName", username);
                        command.Parameters.AddWithValue("@Email", user.Email);
                        command.Parameters.AddWithValue("@Password", hashedPassword); 
                        command.Parameters.AddWithValue("@Birthday", user.Birthday);
                        command.Parameters.AddWithValue("@Gender", user.Gender);
                        command.Parameters.AddWithValue("@Phone", user.PhoneNumber);
                        
                       

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            // ✅ Send email after successful registration
                            string subject = "Welcome to YummyFood!";
                            string message = $"<h3>Hi {username},</h3><p>Thank you for registering on YummyFood. Enjoy our services!</p>";
                            await _emailService.SendEmailAsync(user.Email, subject, message);

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

        private string GenerateUserId()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
        }
    }
}

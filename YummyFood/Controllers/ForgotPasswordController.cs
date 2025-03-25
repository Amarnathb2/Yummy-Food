using Microsoft.AspNetCore.Mvc;
using YummyFood.Models;
using Microsoft.Data.SqlClient;
namespace YummyFood.Controllers
{
    public class ForgotPasswordController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        public ForgotPasswordController(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpGet]
        [Route("/forgotpassword")]
        public ActionResult Index()
        {
            return View("~/Views/ForgotPassword/ForgotPassword.cshtml");
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT UserId FROM [User] WHERE Email = @Email";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    object userId = await command.ExecuteScalarAsync();

                    if (userId != null)
                    {
                        string resetToken = Guid.NewGuid().ToString(); // Generate a unique token
                        string resetLink = Url.Action("ResetPassword", "ForgotPassword", new { token = resetToken }, Request.Scheme);

                        // Store token in database (simplified for example)
                        string updateQuery = "UPDATE [User] SET ResetToken = @ResetToken WHERE Email = @Email";
                        using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@ResetToken", resetToken);
                            updateCommand.Parameters.AddWithValue("@Email", email);
                            await updateCommand.ExecuteNonQueryAsync();
                        }

                        // Send reset email
                        string subject = "Password Reset Request";
                        string message = $"Click the link below to reset your password:<br><a href='{resetLink}'>Reset Password</a>";
                        await _emailService.SendEmailAsync(email, subject, message);
                        
                        // ✅ Instead of returning Ok(), use ViewBag to show the message
                        ViewBag.Message = "Password reset link has been sent to your email.";
                        ViewBag.MessageType = "success"; // To style the message in UI
                        return View("~/Views/ForgotPassword/ForgotPassword.cshtml");
                    }
                }
            }

            ViewBag.Message = "Email not found. Please enter a registered email.";
            ViewBag.MessageType = "error";
            return View("~/Views/ForgotPassword/ForgotPassword.cshtml");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            return View("~/Views/ForgotPassword/ResetPassword.cshtml", model: token);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string newPassword)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT UserId FROM [User] WHERE ResetToken = @ResetToken";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ResetToken", token);
                    object userId = await command.ExecuteScalarAsync();

                    if (userId != null)
                    {
                        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

                        string updateQuery = "UPDATE [User] SET Password = @Password, ResetToken = NULL WHERE ResetToken = @ResetToken";
                        using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@Password", hashedPassword);
                            updateCommand.Parameters.AddWithValue("@ResetToken", token);
                            await updateCommand.ExecuteNonQueryAsync();
                        }

                        return Redirect("/");
                    }
                }
            }

            return BadRequest("Invalid or expired token.");
        }
    }
}
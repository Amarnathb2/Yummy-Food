using System.Data.SqlClient;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto.Generators;

namespace YummyFood.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpPost]
        public async Task<IActionResult> AccountLoginAsync(string email, string password)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT UserId, UserName, Password, Role FROM [User] WHERE Email = @Email";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string userId = reader["UserId"].ToString();
                                string userRole = reader["Role"].ToString();
                                string username = reader["UserName"].ToString();
                                string hashedPassword = reader["Password"].ToString();

                                // Verify hashed password
                                if (BCrypt.Net.BCrypt.Verify(password, hashedPassword))
                                {
                                    // ✅ Create Authentication Cookie
                                    var claims = new List<Claim>
                                    {
                                        new Claim(ClaimTypes.NameIdentifier, userId),
                                        new Claim(ClaimTypes.Name, username),
                                        new Claim(ClaimTypes.Email, email),
                                        new Claim("UserId", userId),
                                        new Claim(ClaimTypes.Role, userRole)// Custom claim
                                    };
                                   
                                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                                    var authProperties = new AuthenticationProperties
                                    {
                                        IsPersistent = true // Remember user session
                                    };

                                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                        new ClaimsPrincipal(claimsIdentity), authProperties);

                                    return Redirect("/");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }

            ViewBag.Error = "Invalid email or password.";
            return Redirect("/");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/");
        }
    }
}

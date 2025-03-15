using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using YummyFood.Models;

namespace YummyFood.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        _logger = logger;
        this._configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet]
    [Route("/test")]
    public IActionResult User()
    {
        string connectionString = _configuration.GetConnectionString("DefaultConnection");
        List<User> users = new List<User>();

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT UserName, UserEmail, Phone FROM [User]", conn);
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                users.Add(new User
                {
                    UserName = reader["UserName"] != DBNull.Value ? reader["UserName"].ToString() : "N/A",
                    UserEmail = reader["UserEmail"] != DBNull.Value ? reader["UserEmail"].ToString() : "N/A",
                    Phone = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : "N/A",
                });
            }

            return View(users);
        }
    }
}
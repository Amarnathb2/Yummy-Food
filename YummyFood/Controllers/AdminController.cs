using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using YummyFood.Models;

namespace YummyFood.Controllers
{
    public class AdminController : Controller
    {
        private readonly IConfiguration _configuration;

        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View("~/Views/Admin/AdminDashboard.cshtml");
        }
        
        #region Setting
        [HttpGet]
        [Route("/setting")]
        public IActionResult Setting()
        {
            List<User> users = GetUsers();
            return View("~/Views/Admin/Setting.cshtml", users);
        }
        #endregion

        #region GetUsers
        private List<User> GetUsers()
        {
            List<User> userList = new List<User>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM YummyFood.dbo.[User]";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    userList.Add(new User
                    {
                        UserId = reader.GetString(reader.GetOrdinal("UserId")),
                        UserName = reader.GetString(reader.GetOrdinal("UserName")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        Password = reader.GetString(reader.GetOrdinal("Password")),
                        Birthday = reader.GetDateTime(reader.GetOrdinal("Birthday")),
                        Gender = reader.GetString(reader.GetOrdinal("Gender")),
                        PhoneNumber = reader.GetString(reader.GetOrdinal("Phone")),
                        Role = reader.GetString(reader.GetOrdinal("Role"))
                    });
                }
            }
            return userList;
        }
        #endregion
       
        #region GetEdit
        [HttpGet]
        [Route("/admin/edit/{id}")]
        public IActionResult Edit(string id)
        {
            User user = GetUserById(id);
            return View("~/Views/Admin/EditUser.cshtml", user);
        }
        #endregion

        #region PostEdit
        [HttpPost]
        [Route("/admin/edit/{id}")]
        public IActionResult Edit(User user)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE YummyFood.dbo.[User] SET UserName=@UserName, Email=@Email, Phone=@Phone, Role=@Role WHERE UserId=@UserId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", user.UserId);
                cmd.Parameters.AddWithValue("@UserName", user.UserName);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@Phone", user.PhoneNumber);
                cmd.Parameters.AddWithValue("@Role", user.Role);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Setting");
        }
        #endregion

        #region GetUserById
        private User GetUserById(string id)
        {
            User user = null;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM YummyFood.dbo.[User] WHERE UserId=@UserId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", id);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    user = new User
                    {
                        UserId = reader.GetString(reader.GetOrdinal("UserId")),
                        UserName = reader.GetString(reader.GetOrdinal("UserName")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        PhoneNumber = reader.GetString(reader.GetOrdinal("Phone")),
                        Role = reader.GetString(reader.GetOrdinal("Role"))
                    };
                }
            }
            return user;
        }
        #endregion

        #region Delete
        [HttpGet]
        [Route("/admin/delete/{id}")]
        public IActionResult Delete(string id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM YummyFood.dbo.[User] WHERE UserId=@UserId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Setting");
        }
        #endregion
       
        #region AddItems
        [HttpGet]
        [Route("/add-items")]
        public IActionResult AddItems()
        {
            return View("~/Views/Admin/AddItems.cshtml");
        }
        #endregion
    }
}

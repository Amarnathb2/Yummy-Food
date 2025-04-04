using System.Data.SqlClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using YummyFood.Models;

namespace YummyFood.Controllers
{
    public class FoodItemController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public FoodItemController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }
        #region AddItems
        [HttpGet]
        [Route("/add-items")]
        public IActionResult AddItems()
        {
            return View("~/Views/Admin/AddItems.cshtml");
        }
        #endregion
        
        // Handle form submission & insert into database
        [HttpPost]
        public async Task<IActionResult> AddItemAsync(FoodItemViewModel item, IFormFile ItemImageFile)
        {
            ModelState.Remove("ItemImage");
            if (!ModelState.IsValid)
            {
                
                return View(item);
            }

            string imagePath = null; // Stores the image path for DB

            if (ItemImageFile != null && ItemImageFile.Length > 0)
            {
                // Validate file type (only images)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(ItemImageFile.FileName).ToLower();

                if (!Array.Exists(allowedExtensions, ext => ext == fileExtension))
                {
                    ModelState.AddModelError("ItemImage", "Invalid file type. Only JPG, PNG, and GIF allowed.");
                    return View(item);
                }

                // Generate unique file name
                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                // Define local path (YummyFood\Uploads\FoodItems\)
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", "FoodItems");

                // Create directory if not exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Full path to save image
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save image file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ItemImageFile.CopyToAsync(fileStream);
                }

                // Store only relative path in DB
                imagePath = $"Uploads/FoodItems/{uniqueFileName}";
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "INSERT INTO FoodItem (ItemType, ItemName, ItemImage, ItemDetails, ItemPrice) VALUES (@ItemType, @ItemName, @ItemImage, @ItemDetails, @ItemPrice)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ItemType", item.ItemType);
                        command.Parameters.AddWithValue("@ItemName", item.ItemName);
                        command.Parameters.AddWithValue("@ItemImage", imagePath ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ItemDetails", item.ItemDetails);
                        command.Parameters.AddWithValue("@ItemPrice", item.ItemPrice);

                        int result = await command.ExecuteNonQueryAsync();

                        if (result > 0)
                        {
                            TempData["SuccessMessage"] = "Food item added successfully!";
                            return View("~/Views/Admin/AddItems.cshtml");
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error: {ex.Message}";
                    return View("~/Views/Admin/AddItems.cshtml");
                }
            }

            return View(item);
        }

        #region ViewItems
        [HttpGet]
        [Route("/view-items")]
        public IActionResult ViewItems()
        {
            List<FoodItemViewModel> foodItems = new List<FoodItemViewModel>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM FoodItem";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    foodItems.Add(new FoodItemViewModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        ItemType = reader.GetString(reader.GetOrdinal("ItemType")),
                        ItemName = reader.GetString(reader.GetOrdinal("ItemName")),
                        ItemImage = reader.IsDBNull(reader.GetOrdinal("ItemImage")) ? null : reader.GetString(reader.GetOrdinal("ItemImage")),
                        ItemDetails = reader.GetString(reader.GetOrdinal("ItemDetails")),
                        ItemPrice = reader.GetString(reader.GetOrdinal("ItemPrice"))
                    });
                }
            }

            return View("~/Views/Admin/ViewItems.cshtml", foodItems);
        }

        #endregion

        #region GetItems
        private List<FoodItemViewModel> GetItems()
        {
            List<FoodItemViewModel> itemList = new List<FoodItemViewModel>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM YummyFood.dbo.FoodItem";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    itemList.Add(new FoodItemViewModel
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ItemType = reader["ItemType"].ToString(),
                        ItemName = reader["ItemName"].ToString(),
                        ItemImage = reader["ItemImage"]?.ToString(),
                        ItemDetails = reader["ItemDetails"].ToString(),
                        ItemPrice = reader["ItemPrice"].ToString(),
                    });
                }
            }

            return itemList;
        }
        #endregion

        #region EditItem [GET]
        [HttpGet]
        [Route("/admin/items/edit/{id}")]
        public IActionResult Edit(int id)
        {
            var item = GetItemById(id);
            return View("~/Views/Admin/EditItem.cshtml", item);
        }
        #endregion

        #region EditItem [POST]
        [HttpPost]
        [Route("/admin/items/edit/{id}")]
        public IActionResult Edit(FoodItemViewModel item)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"UPDATE YummyFood.dbo.FoodItem 
                             SET ItemType=@ItemType, ItemName=@ItemName, 
                                 ItemDetails=@ItemDetails, ItemPrice=@ItemPrice 
                             WHERE Id=@Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", item.Id);
                cmd.Parameters.AddWithValue("@ItemType", item.ItemType);
                cmd.Parameters.AddWithValue("@ItemName", item.ItemName);
                cmd.Parameters.AddWithValue("@ItemDetails", item.ItemDetails);
                cmd.Parameters.AddWithValue("@ItemPrice", item.ItemPrice);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("ViewItems");
        }
        #endregion

        #region DeleteItem
        [HttpGet]
        [Route("/admin/items/delete/{id}")]
        public IActionResult Delete(int id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM YummyFood.dbo.FoodItem WHERE Id=@Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("ViewItems");
        }
        #endregion

        #region GetItemById
        private FoodItemViewModel GetItemById(int id)
        {
            FoodItemViewModel item = null;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM YummyFood.dbo.FoodItem WHERE Id=@Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    item = new FoodItemViewModel
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ItemType = reader["ItemType"].ToString(),
                        ItemName = reader["ItemName"].ToString(),
                        ItemImage = reader["ItemImage"]?.ToString(),
                        ItemDetails = reader["ItemDetails"].ToString(),
                        ItemPrice = reader["ItemPrice"].ToString()
                    };
                }
            }

            return item;
        }
        #endregion
    }
}

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
    }
}

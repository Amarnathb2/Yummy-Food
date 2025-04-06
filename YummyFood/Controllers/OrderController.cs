using System.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using YummyFood.Models;

namespace YummyFood.Controllers
{
    public class OrderController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        public OrderController(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpGet]
        [Route("/order")]
        public IActionResult OrderNow(string itemType = "All")
        {
            var foodItems = new List<Food>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Base query
                string query = "SELECT [Id], [ItemName], [ItemImage], [ItemDetails], [ItemPrice] FROM FoodItem";

                if (!string.IsNullOrEmpty(itemType) && itemType != "All")
                {
                    query += " WHERE ItemType = @ItemType";
                }

                SqlCommand cmd = new SqlCommand(query, conn);

                if (!string.IsNullOrEmpty(itemType) && itemType != "All")
                {
                    cmd.Parameters.AddWithValue("@ItemType", itemType);
                }

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        foodItems.Add(new Food
                        {
                            Id = reader["Id"].ToString(),
                            ItemName = reader["ItemName"].ToString(),
                            ItemImage = reader["ItemImage"].ToString(),
                            ItemDetails = reader["ItemDetails"].ToString(),
                            ItemPrice = Convert.ToInt32(reader["ItemPrice"])
                        });
                    }
                }
            }

            ViewBag.SelectedFilter = itemType; // Pass selected filter to view
            return View("OrderNow", foodItems);
        }


        [HttpPost]
        public async Task<IActionResult> PlaceOrderAsync(List<Food> Items)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Optional: fetch user's email if not stored in Claims
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Redirect("/login");
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            decimal totalAmount = 0;
            var selectedItems = Items.FindAll(i => i.Quantity > 0);

            if (selectedItems.Count == 0)
            {
                return RedirectToAction("OrderNow");
            }

            foreach (var item in selectedItems)
            {
                totalAmount += item.ItemPrice * item.Quantity;
            }

            int orderId;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Insert into Orders table
                string insertOrder = @"INSERT INTO Orders (UserId, OrderDate, TotalAmount, Status)
                                   OUTPUT INSERTED.Id
                                   VALUES (@UserId, @OrderDate, @TotalAmount, @Status)";
                using (SqlCommand cmd = new SqlCommand(insertOrder, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                    cmd.Parameters.AddWithValue("@Status", "Placed");

                    var result = cmd.ExecuteScalar();
                    orderId = result != DBNull.Value && result != null ? Convert.ToInt32(result) : 0;

                }

                // Insert order details
                foreach (var item in selectedItems)
                {
                    string insertDetail = @"INSERT INTO OrderDetails (OrderId, FoodId, Quantity, Price)
                                        VALUES (@OrderId, @FoodId, @Quantity, @Price)";
                    using (SqlCommand cmd = new SqlCommand(insertDetail, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderId", orderId);
                        cmd.Parameters.AddWithValue("@FoodId", item.Id);
                        cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        cmd.Parameters.AddWithValue("@Price", item.ItemPrice);

                        cmd.ExecuteNonQuery();
                    }
                }
            }

            var order = new Order
            {
                Id = orderId,
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                Status = OrderStatus.Placed
            };
            if (orderId > 0)
            {
                string subject = "YummyFood Order Confirmation";
                string message = $@"
        <h3>Hi,</h3>
        <p>Thank you for placing your order with <strong>YummyFood</strong>.</p>
        <p>Your Order ID is <strong>{orderId}</strong> and the total amount is ₹<strong>{totalAmount}</strong>.</p>
        <p>We are preparing your food with love! 🍕🍔</p>
        <p><em>Bon appétit!</em></p>";
               
                if (!string.IsNullOrEmpty(userEmail))
                {
                    await _emailService.SendEmailAsync(userEmail, subject, message);
                }
            }

            return View("OrderSuccess", order);
        }

        [HttpGet]
        [Route("/myorders")]
        public IActionResult MyOrders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Redirect("/login");
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            List<OrderSummaryViewModel> orders = new List<OrderSummaryViewModel>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT o.Id AS OrderId, o.OrderDate, o.TotalAmount, o.Status,
                                od.FoodId, f.ItemName, f.ItemPrice, od.Quantity
                         FROM Orders o
                         LEFT JOIN OrderDetails od ON o.Id = od.OrderId
                         LEFT JOIN FoodItem f ON od.FoodId = f.Id
                         WHERE o.UserId = @UserId
                         ORDER BY o.OrderDate DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    var orderDict = new Dictionary<int, OrderSummaryViewModel>();

                    while (reader.Read())
                    {
                        int orderId = Convert.ToInt32(reader["OrderId"]);

                        if (!orderDict.ContainsKey(orderId))
                        {
                            orderDict[orderId] = new OrderSummaryViewModel
                            {
                                OrderId = orderId,
                                OrderDate = Convert.ToDateTime(reader["OrderDate"]),
                                TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                                Status = reader["Status"].ToString(),
                                Items = new List<OrderedItem>()
                            };
                        }

                        if (reader["FoodId"] != DBNull.Value)
                        {
                            orderDict[orderId].Items.Add(new OrderedItem
                            {
                                ItemName = reader["ItemName"].ToString(),
                                Price = Convert.ToDecimal(reader["ItemPrice"]),
                                Quantity = Convert.ToInt32(reader["Quantity"])
                            });
                        }
                    }

                    orders = orderDict.Values.ToList();
                }
            }

            return View(orders);
        }

    }
}

using Microsoft.AspNetCore.Mvc;

namespace YummyFood.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Admin/AdminDashboard.cshtml");
        }
    }
}

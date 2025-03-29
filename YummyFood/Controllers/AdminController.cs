using Microsoft.AspNetCore.Mvc;

namespace YummyFood.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Admin/AdminDashboard.cshtml");
        }
        [HttpGet]
        [Route("/setting")]
        public IActionResult Setting()
        {
            return View("~/Views/Admin/Setting.cshtml");
        }

        [HttpGet]
        [Route("/add-items")]
        public IActionResult AddItems()
        {
            return View("~/Views/Admin/AddItems.cshtml");
        }
    }
}

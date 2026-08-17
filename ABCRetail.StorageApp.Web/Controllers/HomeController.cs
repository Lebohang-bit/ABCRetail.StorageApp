using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ABCRetail.StorageApp.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(Dashboard));
        }
    }
}
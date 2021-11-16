using Microsoft.AspNetCore.Mvc;

namespace DevExtreme.Demo.Controllers
{
    public class AboutController : Controller
    {
        private readonly ILogger<AboutController> _logger;

        public AboutController(ILogger<AboutController> logger)
        {
            _logger = logger;
        }

        public IActionResult DataGrid()
        {
            return View();
        }

        public IActionResult AjaxDataSource()
        {
            return View();
        }
    }
}
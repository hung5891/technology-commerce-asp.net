using Microsoft.AspNetCore.Mvc;

namespace TechnologyCommerce.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

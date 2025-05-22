using Microsoft.AspNetCore.Mvc;

namespace TechnologyCommerce.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace TechnologyCommerce.Controllers
{
    public class CheckOutController:Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

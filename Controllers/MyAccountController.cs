using Microsoft.AspNetCore.Mvc;

namespace TechnologyCommerce.Controllers
{
    public class MyAccountController:Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

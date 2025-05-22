using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechnologyCommerce.Dbcontext;

// using TechnologyCommerce.Data; // Remove or update this line

// Replace with the correct namespace for your data context and ApplicationUser
using TechnologyCommerce.Models; // Example: adjust 'Models' to your actual namespace

[Authorize]
public class ProfileController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        var orders = await _context.Orders
            .Where(o => o.UserId == user.Id && o.Status == "Paid")
            .Select(o => new Ordertest
            {
                Id = (int)o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status
            })
            .ToListAsync();

        var model = new ProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            Orders = orders
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user != null)
        {
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Message"] = "Profile updated successfully.";
                return RedirectToAction("Index");
            }
        }

        TempData["Error"] = "Failed to update profile.";
        return RedirectToAction("Index");
    }
}
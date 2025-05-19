using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TechnologyCommerce.Models;
using TechnologyCommerce.Dbcontext;
using TechnologyCommerce.ViewModels;
using TechnologyCommerce.Extensions;
using Microsoft.EntityFrameworkCore;

public class CartSummaryViewComponent : ViewComponent
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartSummaryViewComponent(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        List<CartItem> cartItems = new List<CartItem>();
        var userId = _userManager.GetUserId(HttpContext.User);

        if (userId != null)
        {
            // Đã đăng nhập: lấy cart từ database
            cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.Cart.UserId == userId)
                .ToListAsync();
        }
        else
        {
            // Chưa đăng nhập: lấy cart từ Session
            cartItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            // Lấy thông tin sản phẩm cho từng CartItem
            foreach (var item in cartItems)
            {
                item.Product = await _context.Products.FindAsync(item.ProductId);
            }
        }

        int totalQuantity = cartItems.Sum(x => x.Quantity);
        decimal totalPrice = cartItems.Sum(x => x.Product != null ? x.Product.Price * x.Quantity : 0);

        var vm = new CartSummaryViewModel
        {
            CartItems = cartItems,
            TotalQuantity = totalQuantity,
            TotalPrice = totalPrice
        };

        return View(vm);
    }
}
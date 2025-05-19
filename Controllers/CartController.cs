using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechnologyCommerce.Dbcontext;
using TechnologyCommerce.Models;
using TechnologyCommerce.Extensions;
namespace TechnologyCommerce.Controllers;

public class CartController : Controller
{
    private readonly ILogger<CartController> _logger;
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(ILogger<CartController> logger, AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
        {
            // Lấy cart từ Session cho user chưa đăng nhập
            var cartItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            // Lấy thông tin sản phẩm cho từng CartItem nếu cần
            foreach (var item in cartItems)
            {
                item.Product = await _context.Products.FindAsync(item.ProductId);
            }

            var cart = new Cart
            {
                CartItems = cartItems,
                TotalPrice = cartItems.Sum(ci => ci.Product != null ? ci.Product.Price * ci.Quantity : 0)
            };

            return View(cart);
        }

        // Đã đăng nhập: lấy cart từ DB như cũ
        var dbCart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (dbCart == null)
        {
            dbCart = new Cart
            {
                UserId = userId,
                TotalPrice = 0,
                CreatedDate = DateTime.Now,
                CartItems = new List<CartItem>()
            };

            _context.Carts.Add(dbCart);
            await _context.SaveChangesAsync();
        }

        return View(dbCart);
    }

    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
        {
            // Lưu giỏ hàng vào Session cho user chưa đăng nhập
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var cartItem = cart.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem == null)
            {
                cart.Add(new CartItem { ProductId = productId, Quantity = quantity });
            }
            else
            {
                cartItem.Quantity += quantity;
            }
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index", "Store");
        }

        // Đã đăng nhập: lưu vào database như cũ
        var dbCart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (dbCart == null)
        {
            dbCart = new Cart
            {
                UserId = userId,
                TotalPrice = 0,
                CreatedDate = DateTime.Now,
                CartItems = new List<CartItem>()
            };

            _context.Carts.Add(dbCart);
            await _context.SaveChangesAsync();
        }

        var dbCartItem = dbCart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

        if (dbCartItem == null)
        {
            dbCartItem = new CartItem
            {
                CartId = dbCart.Id,
                ProductId = productId,
                Quantity = quantity
            };
            dbCart.CartItems.Add(dbCartItem);
        }
        else
        {
            dbCartItem.Quantity += quantity;
        }

        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            dbCart.TotalPrice += product.Price * quantity;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Store");
    }

    [HttpPost]
    public IActionResult RemoveItem(int productId)
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null)
        {
            // Xóa khỏi Session cho user chưa đăng nhập
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(ci => ci.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index", "Home");
        }

        // Đã đăng nhập: xóa khỏi DB
        var dbCart = _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefault(c => c.UserId == userId);

        if (dbCart != null)
        {
            var dbItem = dbCart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (dbItem != null)
            {
                dbCart.CartItems.Remove(dbItem);
                _context.CartItems.Remove(dbItem);
                dbCart.TotalPrice = dbCart.CartItems.Sum(ci => ci.Product != null ? ci.Product.Price * ci.Quantity : 0);
                _context.SaveChanges();
            }
        }
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

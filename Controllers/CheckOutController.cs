using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TechnologyCommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechnologyCommerce.Dbcontext;
using TechnologyCommerce.Extensions;
namespace TechnologyCommerce.Controllers
{

    public class CheckOutController : Controller
    {
        private readonly ILogger<CheckOutController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckOutController(ILogger<CheckOutController> logger, AppDbContext context, UserManager<ApplicationUser> userManager)
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
                foreach (var item in cartItems)
                    item.Product = await _context.Products.FindAsync(item.ProductId);

                var cart = new Cart
                {
                    CartItems = cartItems,
                    TotalPrice = cartItems.Sum(ci => ci.Product != null ? ci.Product.Price * ci.Quantity : 0)
                };

                if (!cart.CartItems.Any())
                {
                    // Nếu giỏ hàng rỗng, chuyển hướng về trang giỏ hàng
                    return RedirectToAction("Index", "Home");
                }

                return View(cart);
            }

            // Đã đăng nhập: lấy cart từ DB như cũ
            var dbCart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (dbCart == null || !dbCart.CartItems.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            return View(dbCart);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder()
        {
            // Lấy UserId của người dùng đang đăng nhập
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập trước khi thanh toán.";
                return RedirectToAction("Login", "Home"); // hoặc "Account" tùy controller login của bạn
            }

            // Lấy giỏ hàng của người dùng
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                // Nếu giỏ hàng rỗng, chuyển hướng về trang giỏ hàng
                return RedirectToAction("Index", "Home");
            }

            // Tạo đơn hàng mới
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cart.TotalPrice,
                Status = "Pending",
                OrderItems = cart.CartItems.Select(ci => new OrderDetail
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Product.Price
                }).ToList()
            };

            // Lưu đơn hàng vào cơ sở dữ liệu
            _context.Orders.Add(order);

            // Xóa giỏ hàng sau khi tạo đơn hàng
            _context.CartItems.RemoveRange(cart.CartItems);
            cart.CartItems.Clear();
            cart.TotalPrice = 0;

            await _context.SaveChangesAsync();

            // Chuyển hướng đến trang "Thank You"
            return RedirectToAction("Index", "Thankyou");
        }
        public IActionResult OrderConfirmation(int orderId)
        {
            // Hiển thị trang xác nhận đơn hàng
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return RedirectToAction("Index", "Home"); // Nếu không tìm thấy đơn hàng, chuyển hướng về trang chủ
            }

            return View(order);
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
}

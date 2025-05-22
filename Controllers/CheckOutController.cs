using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TechnologyCommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechnologyCommerce.Dbcontext;
using TechnologyCommerce.Extensions;
using TechnologyCommerce.Services;
using TechnologyCommerce.ViewModels;
using TechnologyCommerce.Helpers;

using TechnologyCommerce.ViewModel;
using Microsoft.AspNetCore.Authorization; // Add this if VnPaymentRequestModel is in ViewModels namespace

namespace TechnologyCommerce.Controllers
{

    public class CheckOutController : Controller
    {
        private readonly ILogger<CheckOutController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVnPayService _vnPayService;
        public CheckOutController(ILogger<CheckOutController> logger, AppDbContext context, UserManager<ApplicationUser> userManager, IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
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
        public async Task<IActionResult> CreateOrder(string PaymentMethod)
        {
            // Lấy UserId của người dùng đang đăng nhập
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập trước khi thanh toán.";
                return RedirectToAction("Login", "Home");
            }

            // Lấy giỏ hàng của người dùng
            var cart = await GetCartAsync(userId);
            if (cart == null || !cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Home");
            }

            // Tạo đơn hàng
            var order = await CreateOrderAsync(userId, cart);

            // Xử lý theo hình thức thanh toán
            return PaymentMethod switch
            {
                "VNPay" => await HandleVNPayPayment(cart),
                "COD" => await HandleCODPayment(order, cart),
                _ => RedirectToAction("Index", "CheckOut")
            };
        }
        private async Task<IActionResult> HandleVNPayPayment(Cart cart)
        {
            // Tạo thông tin đơn hàng tạm thời
            var tempOrder = new
            {
                UserId = _userManager.GetUserId(User),
                OrderDate = DateTime.Now,
                TotalAmount = cart.TotalPrice,
                Status = "Pending",
                OrderItems = cart.CartItems.Select(ci => new
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Product.Price
                }).ToList()
            };

            // Mã hóa thông tin đơn hàng tạm thời (hoặc lưu vào session)
            var tempOrderJson = System.Text.Json.JsonSerializer.Serialize(tempOrder);
            HttpContext.Session.SetString("TempOrder", tempOrderJson);

            // Tạo URL thanh toán VNPay
            var vnPayModel = new VnPaymentRequestModel
            {
                Amount = (double)cart.TotalPrice,
                CreatedDate = DateTime.Now,
                Description = "Thanh toán đơn hàng",
                FullName = User.Identity.Name,
                OrderId = Guid.NewGuid().ToString() // Sử dụng mã tham chiếu duy nhất
            };

            var paymentUrl = _vnPayService.CreatePaymentUrl(HttpContext, vnPayModel);
            return Redirect(paymentUrl);
        }
        private async Task<IActionResult> HandleCODPayment(Order order, Cart cart)
        {
            // Xóa giỏ hàng sau khi tạo đơn hàng
            _context.CartItems.RemoveRange(cart.CartItems);
            cart.CartItems.Clear();
            cart.TotalPrice = 0;

            await _context.SaveChangesAsync();

            // Chuyển hướng đến trang xác nhận đơn hàng
            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }
        private async Task<Order> CreateOrderAsync(string userId, Cart cart)
        {
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

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }
        private async Task<Cart> GetCartAsync(string userId)
        {
            var cart = await _context.Carts
        .Include(c => c.CartItems)
        .ThenInclude(ci => ci.Product)
        .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                // Tính tổng tiền
                cart.TotalPrice = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity);
            }

            return cart;
        }
        public IActionResult OrderConfirmation(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index", "Home");
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

        [Authorize]
        public IActionResult PaymentFail()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> PaymentCallBack()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            if (response == null)
            {
                TempData["ErrorMessage"] = "Không nhận được phản hồi từ VNPay.";
                return RedirectToAction("PaymentFail");
            }

            if (response.VnPayResponseCode != "00")
            {
                TempData["ErrorMessage"] = $"Thanh toán không thành công. Mã lỗi: {response.VnPayResponseCode}.";
                return RedirectToAction("PaymentFail");
            }

            // Lấy thông tin đơn hàng tạm thời từ session
            var tempOrderJson = HttpContext.Session.GetString("TempOrder");
            if (string.IsNullOrEmpty(tempOrderJson))
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin đơn hàng.";
                return RedirectToAction("PaymentFail");
            }

            // Deserialize JSON thành TempOrder
            var tempOrder = System.Text.Json.JsonSerializer.Deserialize<TempOrder>(tempOrderJson);

            // Lưu đơn hàng vào cơ sở dữ liệu
            var order = new Order
            {
                UserId = tempOrder.UserId,
                OrderDate = tempOrder.OrderDate,
                TotalAmount = tempOrder.TotalAmount,
                Status = "Paid", // Đánh dấu là đã thanh toán
                OrderItems = tempOrder.OrderItems.Select(item => new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Xóa giỏ hàng của người dùng
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == order.UserId);

            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                cart.CartItems.Clear();
                cart.TotalPrice = 0;
            }

            await _context.SaveChangesAsync();

            // Xóa thông tin đơn hàng tạm thời khỏi session
            HttpContext.Session.Remove("TempOrder");

            TempData["Message"] = "Thanh toán VNPay thành công.";
            return RedirectToAction("PaymentSuccess");
        }
        public IActionResult PaymentSuccess()
        {
            return View();
        }
    }
}

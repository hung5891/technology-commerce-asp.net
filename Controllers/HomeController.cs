using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechnologyCommerce.Dbcontext;
using TechnologyCommerce.Models;
using TechnologyCommerce.ViewModel;
using Microsoft.EntityFrameworkCore;
namespace TechnologyCommerce.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    public HomeController(ILogger<HomeController> logger, AppDbContext context,
    UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }





    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    [HttpGet]
    public IActionResult R()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "This account has been locked out.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
        }

        return View(model);
    }

    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Address = model.Address
            };

            // Tạo người dùng mới
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Gán Role "USER" cho người dùng
                var roleResult = await _userManager.AddToRoleAsync(user, "USER");
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                // Đăng nhập người dùng sau khi đăng ký thành công
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            // Xử lý lỗi nếu tạo người dùng thất bại
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // Nếu ModelState không hợp lệ, trả về lại view với dữ liệu hiện tại
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult Catagories()
    {
        return View();
    }
    public IActionResult Laptop()
    {
        return View();
    }
    public IActionResult Network()
    {
        return View();
    }

    public IActionResult Mobile()
    {
        return View();
    }
    public IActionResult Accessories()
    {
        return View();
    }
    public IActionResult Cart()
    {
        return View();
    }
    public async Task<IActionResult> Carts()
    {
        // Lấy UserId của người dùng đang đăng nhập
        var userId = _userManager.GetUserId(User);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account"); // Chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
        }

        // Lấy giỏ hàng từ cơ sở dữ liệu, bao gồm các mục trong giỏ hàng và thông tin sản phẩm
        var cart = await _context.Carts
            .Include(c => c.CartItems) // Bao gồm các mục trong giỏ hàng
            .ThenInclude(ci => ci.Product) // Bao gồm thông tin sản phẩm trong mỗi mục
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            // Nếu giỏ hàng chưa tồn tại, tạo mới
            cart = new Cart
            {
                UserId = userId,
                TotalPrice = 0,
                CreatedDate = DateTime.Now,
                CartItems = new List<CartItem>()
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return View(cart); // Truyền giỏ hàng đến view
    }
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        // Lấy UserId của người dùng đang đăng nhập
        var userId = _userManager.GetUserId(User);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account"); // Chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
        }

        // Lấy giỏ hàng của người dùng
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            // Nếu giỏ hàng chưa tồn tại, tạo mới
            cart = new Cart
            {
                UserId = userId,
                TotalPrice = 0,
                CreatedDate = DateTime.Now,
                CartItems = new List<CartItem>()
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

        if (cartItem == null)
        {
            // Nếu sản phẩm chưa có trong giỏ hàng, thêm mới
            cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = quantity
            };

            cart.CartItems.Add(cartItem);
        }
        else
        {
            // Nếu sản phẩm đã có trong giỏ hàng, tăng số lượng
            cartItem.Quantity += quantity;
        }

        // Lấy thông tin sản phẩm để cập nhật tổng giá trị giỏ hàng
        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            cart.TotalPrice += product.Price * quantity;
        }

        // Lưu thay đổi vào cơ sở dữ liệu
        await _context.SaveChangesAsync();

        return RedirectToAction("Cart"); // Chuyển hướng về trang giỏ hàng
    }
    public async Task<IActionResult> RemoveItem(int id)
    {
        // Lấy UserId của người dùng đang đăng nhập
        var userId = _userManager.GetUserId(User);

        if (userId == null)
        {
            return RedirectToAction("Login", "Account"); // Chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
        }

        // Lấy giỏ hàng của người dùng
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            return RedirectToAction("Cart"); // Nếu giỏ hàng không tồn tại, chuyển hướng về trang giỏ hàng
        }

        // Tìm mục giỏ hàng cần xóa
        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == id);

        if (cartItem != null)
        {
            // Cập nhật tổng giá trị giỏ hàng
            var product = await _context.Products.FindAsync(cartItem.ProductId);
            if (product != null)
            {
                cart.TotalPrice -= product.Price * cartItem.Quantity;
            }

            // Xóa mục giỏ hàng
            cart.CartItems.Remove(cartItem);
            _context.CartItems.Remove(cartItem); // Xóa mục giỏ hàng khỏi cơ sở dữ liệu
        }

        // Lưu thay đổi vào cơ sở dữ liệu
        await _context.SaveChangesAsync();

        return RedirectToAction("Cart"); // Chuyển hướng về trang giỏ hàng
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

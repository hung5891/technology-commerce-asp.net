using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechnologyCommerce.Dbcontext;
using TechnologyCommerce.Models;

public class ProductController : Controller
{
    private readonly AppDbContext _context;

    public ProductController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Product/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(p => p.Category) // Include Category data
            .FirstOrDefaultAsync(m => m.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        // Lấy related products cùng category
        var relatedProducts = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
            .Take(4)
            .ToListAsync();

        ViewBag.RelatedProducts = relatedProducts;


        return View(product);
    }
    [HttpGet]
    public async Task<IActionResult> Search(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            // Nếu không có từ khóa, trả về danh sách rỗng
            return Json(new List<object>());
        }

        // Tìm kiếm sản phẩm theo tên
        var products = await _context.Products
            .Where(p => p.Name.Contains(query)) // Tìm sản phẩm có tên chứa từ khóa
            .Select(p => new
            {
                p.Id,
                p.Name,
                ImageUrl = Url.Content("~/img/" + p.ImageUrl) // Đường dẫn hình ảnh sản phẩm
            })
            .ToListAsync();

        // Trả về kết quả dưới dạng JSON
        return Json(products);
    }

}
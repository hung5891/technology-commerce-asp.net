using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechnologyCommerce.Dbcontext;
using TechnologyCommerce.Models;
using TechnologyCommerce.ViewModel;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;
namespace TechnologyCommerce.Controllers;


public class StoreController : Controller
{
    private readonly AppDbContext _context;
    // Inject DbContext vào controller
    public StoreController(AppDbContext context)
    {
        _context = context;
    }
    public async Task<IActionResult> Index(int? page)
    {
        int pageSize = 9; // Số sản phẩm trên mỗi trang
        int pageNumber = page ?? 1; // Trang hiện tại, mặc định là trang 1

        var products = await _context.Products.ToListAsync();

        // Sử dụng X.PagedList để phân trang
        var pagedProducts = products.ToPagedList(pageNumber, pageSize);

        return View(pagedProducts);
    }
}

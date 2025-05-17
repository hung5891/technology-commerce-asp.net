using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechnologyCommerce.Dbcontext;
using TechnologyCommerce.Models;

namespace TechnologyCommerce.Controllers;

[Route("product")]
public class ProductController : Controller
{
    private readonly ILogger<ProductController> _logger;
    private readonly AppDbContext _context;

    public ProductController(ILogger<ProductController> logger ,AppDbContext context)
    {
        _logger = logger;
        _context = context; // Thêm DbContext
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        // Lấy danh sách sản phẩm từ database
        var products = _context.Products.ToList();
        // Truyền dữ liệu sang view
        return View(products);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost("create")]
    public IActionResult Create(Product product)
    {
        if (ModelState.IsValid)
        {
            // Logic to save the product (e.g., to a database) goes here.
            return RedirectToAction("Index");
        }
        return View("Product/Create", product);
    }
    [HttpGet("details/{id}")]
    public IActionResult Details(int id)
    {
        var product = _context.Products
            
            .FirstOrDefault(p => p.Id == id);

        if (product == null)
            return NotFound();

        return View(product);
    }

    [HttpGet("privacy")]
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
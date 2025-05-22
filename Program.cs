using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechnologyCommerce.Dbcontext;
using TechnologyCommerce.Models;
using TechnologyCommerce.Services;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration["Connectionstrings:ConnectedDb"]));

// Đăng ký Identity với ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<AppDbContext>() // Thêm DbContext vào đây
.AddDefaultTokenProviders();
// Thêm cấu hình này để đổi đường dẫn đăng nhập
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Home/Index";
    options.AccessDeniedPath = "/Home/AccessDenied";
});
// XÓA TẤT CẢ CÁC DÒNG AddAuthentication() THỪA
// KHÔNG CẦN GỌI AddAuthentication() RIÊNG VÌ AddIdentity() ĐÃ TỰ THÊM

builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddSingleton<IVnPayService, VnPayService>();

var app = builder.Build();

// ... Phần còn lại giữ nguyên

// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}")
    .WithStaticAssets();

// Seed dữ liệu Role
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.Initialize(services);
}


app.Run();

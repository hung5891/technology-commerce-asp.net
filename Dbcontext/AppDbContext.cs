using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechnologyCommerce.Models;

namespace TechnologyCommerce.Dbcontext
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>

    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        
       
        public DbSet<Product> Products { get; set; }
       
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
         public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
   
    }
    
}

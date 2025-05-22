using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DocumentFormat.OpenXml.Drawing.Charts;
using TechnologyCommerce.Models;

namespace TechnologyCommerce.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; } // Khóa chính của mục đơn hàng

        [Required]
        public long OrderId { get; set; } // Khóa ngoại liên kết với đơn hàng

        [ForeignKey("OrderId")]
        public Order Order { get; set; } // Điều hướng đến đơn hàng

        [Required]
        public int ProductId { get; set; } // Khóa ngoại liên kết với sản phẩm`

        [ForeignKey("ProductId")]
        public Product Product { get; set; } // Điều hướng đến sản phẩm

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; } // Số lượng sản phẩm

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; } // Giá của sản phẩm tại thời điểm đặt hàng
    }
}

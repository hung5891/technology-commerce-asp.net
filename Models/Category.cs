using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TechnologyCommerce.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; } // Khóa chính

        [Required]
        [StringLength(100)]
        public string Name { get; set; } // Tên danh mục

        // Quan hệ 1-Nhiều với Product
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
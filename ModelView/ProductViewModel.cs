using System.ComponentModel.DataAnnotations;

namespace TechnologyCommerce.ViewModel
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public IFormFile ImageFile { get; set; }
        public string CategoryName { get; set; } // Thêm thuộc tính này

        public int CategoryId { get; set; } // Thêm thuộc tính này

    }
}

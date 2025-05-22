using System.ComponentModel.DataAnnotations;

namespace TechnologyCommerce.ViewModel
{
    public class OrderViewModel
    {
        public long Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now; // Ngày đặt hàng

        public decimal TotalAmount { get; set; } // Tổng giá trị đơn hàng

        public string Status { get; set; }
    }
}

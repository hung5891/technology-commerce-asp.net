
namespace TechnologyCommerce.Helpers
{
    public class TempOrder
    {
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<TempOrderItem> OrderItems { get; set; }
    }

    public class TempOrderItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}


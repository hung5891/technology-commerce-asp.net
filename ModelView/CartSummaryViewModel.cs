namespace TechnologyCommerce.ViewModels
{
    public class CartSummaryViewModel
    {
        public List<TechnologyCommerce.Models.CartItem> CartItems { get; set; } = new List<TechnologyCommerce.Models.CartItem>();
        public int TotalQuantity { get; set; }
        public decimal TotalPrice { get; set; }

    }
}
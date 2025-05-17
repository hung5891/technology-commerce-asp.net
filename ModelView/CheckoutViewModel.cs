using System.ComponentModel.DataAnnotations;
using TechnologyCommerce.Models;

namespace TechnologyCommerce.ViewModel
{
    public class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; }
        public decimal TotalPrice { get; set; }
    }
}

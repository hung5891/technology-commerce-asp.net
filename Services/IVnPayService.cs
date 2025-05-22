using System.Net.Http;
using TechnologyCommerce.Models;
using TechnologyCommerce.ViewModel; // Add this line if VnPaymentRequestModel is in the Models namespace

namespace TechnologyCommerce.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model);
        VnPaymentResponseModel PaymentExecute(IQueryCollection colection);
    }
}
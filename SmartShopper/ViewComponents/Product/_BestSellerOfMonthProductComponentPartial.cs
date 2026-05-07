using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.ViewComponents.Product
{
    public class _BestSellerOfMonthProductComponentPartial:ViewComponent
    {
        private readonly IProductService _productService;

        public _BestSellerOfMonthProductComponentPartial(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var products = await _productService.TGetBestSellersOfMonthAsync();
            return View(products);
        }
    }
}

using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.ViewComponents.Product
{
    public class _TopSellingProductListComponentPartial:ViewComponent
    {
        private readonly IProductService _productService;

        public _TopSellingProductListComponentPartial(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var products = await _productService.TTopSellingProductListAsync();
            return View(products);
        }
    }
}

using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.ViewComponents.Product
{
    public class _MostFavoritedProductsComponentPartial:ViewComponent
    {
        private readonly IProductService _productService;

        public _MostFavoritedProductsComponentPartial(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var products = await _productService.TGetMostFavoritedProductsAsync();
            return View(products);
        }
    }
}

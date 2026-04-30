using AspNetCoreGeneratedDocument;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> ProductList()
        {
            var products = await _productService.TGetProductListForIndex();
            return View(products);
        }

        public async Task<IActionResult> ProductDetails(Guid id)
        {
            var product = await _productService.TGetProductDetailsAsync(id);
            return View(product);
        }
    }
}

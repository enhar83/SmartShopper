using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.ViewComponents.Product
{
    public class _SimilarProductsListInProductDetailComponentPartial:ViewComponent
    {
        private readonly IProductService _productService;

        public _SimilarProductsListInProductDetailComponentPartial(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid subCategoryId)
        {
            var similarProducts = await _productService.TGetSimilarProductsForProductDetailAsync(subCategoryId);
            return View(similarProducts);
        }
    }
}

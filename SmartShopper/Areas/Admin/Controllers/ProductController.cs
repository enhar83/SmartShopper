using System.Threading.Tasks;
using Core_Layer.Dtos.CategoryDtos;
using Core_Layer.Dtos.ProductDtos;
using Core_Layer.Exceptions;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> ProductList()
        {
            var products = await _productService.TGetProductListAsync();
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(AddProductDto addProductDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("<br>", errors) });
            }

            try
            {
                await _productService.TAddProductAsync(addProductDto);
                return Json(new { success = true, message = "Product has been added successfully!" });
            }
            catch (LogicException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An unexpected error occurred while saving." });
            }
        }
    }
}

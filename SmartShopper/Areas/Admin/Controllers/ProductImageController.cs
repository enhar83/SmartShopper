using Core_Layer.Dtos.ProductImagesDtos;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductImageController : Controller
    {
        private readonly IProductImageService _productImageService;

        public ProductImageController(IProductImageService productImageService)
        {
            _productImageService = productImageService;
        }

        public async Task<IActionResult> GetProductImages(Guid productId)
        {
            if (productId == Guid.Empty)
                return BadRequest("Invalid Product ID.");

            var images = await _productImageService.TGetProductImagesByProductIdAsync(productId);
            return Json(images);
        }

        [HttpPost]
        public async Task<IActionResult> AddProductImage(AddProductImageDto addProductImageDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("<br>", errors) });
            }

            try
            {
                await _productImageService.TAddProductImageAsync(addProductImageDto);
                return Json(new { success = true, message = "Image uploaded successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred during upload: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProductImage(Guid id)
        {
            try
            {
                await _productImageService.TDeleteProductImageAsync(id);
                return Json(new { success = true, message = "Image has been deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
    }
}

using Core_Layer.Dtos.CategoryDtos;
using Core_Layer.Dtos.SubCategoryDtos;
using Core_Layer.Exceptions;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        private readonly ISubCategoryService _subCategoryService;

        public SubCategoryController(ISubCategoryService subCategoryService)
        {
            _subCategoryService = subCategoryService;
        }

        public async Task<IActionResult> GetSubCategoriesByParent(Guid parentId)
        {
            if (parentId == Guid.Empty)
                return BadRequest("Category ID is required.");

            var subCategories = await _subCategoryService.TGetAllSubCategoriesByParentAsync(parentId);
            return Json(subCategories);
        }

        [HttpPost]
        public async Task<IActionResult> AddSubCategory(AddSubCategoryDto addSubCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("<br>", errors) });
            }

            try
            {
                await _subCategoryService.TAddSubCategoryAsync(addSubCategoryDto);
                return Json(new { success = true, message = "Subcategory has been added successfully!" });
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

        public async Task<IActionResult> GetCategory(Guid id)
        {
            var category = await _subCategoryService.TGetByIdAsync(id);
            if (category == null)
                return NotFound(new { success = false, message = "Subcategory not found." });

            return Json(category);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSubCategory(UpdateSubCategoryDto updateSubCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("<br>", errors) });
            }

            try
            {
                await _subCategoryService.TUpdateSubCategoryAsync(updateSubCategoryDto);
                return Json(new { success = true, message = "Subcategory updated successfully!" });
            }
            catch (LogicException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            // SubCategoryController.cs içinde geçici olarak:
            catch (Exception ex)
            {
                // InnerException genellikle asıl veritabanı hatasını (örn: 'CreatedDate cannot be null') söyler
                var innerError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = "Database Error: " + innerError });
            }
        }
    }
}

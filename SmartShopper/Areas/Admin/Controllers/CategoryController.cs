using Core_Layer.Dtos.CategoryDtos;
using Core_Layer.Exceptions;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> CategoryList()
        {
            var categories = await _categoryService.TGetAllCategories();
            return View(categories);   
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(AddCategoryDto addCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("<br>", errors) });
            }

            try
            {
                await _categoryService.TAddCategory(addCategoryDto);
                return Json(new { success = true, message = "Category has been added successfully!" });
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
            var category = await _categoryService.TGetByIdAsync(id);
            if (category == null)
                return NotFound(new { success = false, message = "Category not found." });

            return Json(category);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(UpdateCategoryDto updateCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("<br>", errors) });
            }

            try
            {
                await _categoryService.TUpdateCategoryAsync(updateCategoryDto);
                return Json(new { success = true, message = "Category updated successfully!" });
            }
            catch (LogicException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred during update." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveCategories()
        {
            var categories = await _categoryService.TGetAllCategories();
            var result = categories.Where(x => !x.IsDeleted).Select(x => new {
                id = x.Id,
                name = x.Name,
                hasGender = x.HasGender
            }).ToList();

            return Json(result);
        }
    }
}

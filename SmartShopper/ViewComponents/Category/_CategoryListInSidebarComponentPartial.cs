using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.ViewComponents.Category
{
    public class _CategoryListInSidebarComponentPartial:ViewComponent
    {
        private readonly ICategoryService _categoryService;

        public _CategoryListInSidebarComponentPartial(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _categoryService.TGetCategoriesForSidebarAsync();
            return View(categories);
        }
    }
}

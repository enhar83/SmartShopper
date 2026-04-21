using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.CategoryDtos;

namespace Core_Layer.IServices
{
    public interface ICategoryService
    {
        Task<List<CategoryListDto>> TGetAllCategories();
        Task TAddCategory(AddCategoryDto addCategoryDto);
    }
}

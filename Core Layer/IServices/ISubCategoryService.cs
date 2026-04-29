using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.CategoryDtos;
using Core_Layer.Dtos.SubCategoryDtos;

namespace Core_Layer.IServices
{
    public interface ISubCategoryService
    {
        Task TAddSubCategoryAsync(AddSubCategoryDto addSubCategoryDto);
        Task<SubCategoryListDtoAdminPanel> TGetByIdAsync(Guid id);
        Task<List<SubCategoryListDtoAdminPanel>> TGetAllSubCategoriesByParentAsync(Guid id);
        Task TUpdateSubCategoryAsync(UpdateSubCategoryDto updateSubCategoryDto);
    }
}

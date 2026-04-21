using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.SubCategoryDtos;

namespace Core_Layer.IServices
{
    public interface ISubCategoryService
    {
        Task TAddSubCategoryAsync(AddSubCategoryDto addSubCategoryDto);
        Task<List<SubCategoryListDto>> TGetAllSubCategoriesByParentAsync(Guid id);
    }
}

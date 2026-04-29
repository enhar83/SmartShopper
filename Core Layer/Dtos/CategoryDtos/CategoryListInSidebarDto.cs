using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.SubCategoryDtos;

namespace Core_Layer.Dtos.CategoryDtos
{
    public class CategoryListInSidebarDto
    {
        public Guid Id { get; set; }
        public required string CategoryName { get; set; }
        public ICollection<SubCategoryListInSidebarDto>? SubCategories { get; set; }
    }
}

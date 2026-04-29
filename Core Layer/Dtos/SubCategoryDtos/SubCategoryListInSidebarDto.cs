using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.SubCategoryDtos
{
    public class SubCategoryListInSidebarDto
    {
        public Guid Id { get; set; }
        public required string SubCategoryName { get; set; }
        public int ProductCount { get; set; }
    }
}

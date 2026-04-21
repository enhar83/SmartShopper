using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer;

namespace Core_Layer.Dtos.SubCategoryDtos
{
    public class AddSubCategoryDto
    {
        public required string Name { get; set; }
        public Guid CategoryId { get; set; }
    }
}

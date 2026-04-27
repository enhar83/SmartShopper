using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.SubCategoryDtos
{
    public class UpdateSubCategoryDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CategoryId { get; set; }
    }
}

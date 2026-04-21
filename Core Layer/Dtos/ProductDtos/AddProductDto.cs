using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer;

namespace Core_Layer.Dtos.ProductDtos
{
    public class AddProductDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public GenderType? Gender { get; set; }
        public List<ProductImage>? ProductImages { get; set; }
        public Guid SubCategoryId { get; set; }
    }
}

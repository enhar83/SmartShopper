using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer;

namespace Core_Layer.Dtos.ProductDtos
{
    public class ProductListDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public GenderType? Gender { get; set; }
        public List<ProductImage>? ProductImages { get; set; }
        public Guid SubCategoryId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsDelete { get; set; }
    }
}

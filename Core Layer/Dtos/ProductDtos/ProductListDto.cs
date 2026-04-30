using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.ProductDtos
{
    public class ProductListDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string CategoryName { get; set; }
        public string? SubCategoryName { get; set; }
        public string? MainImageUrl { get; set; }
        public double Price { get; set; }
        public required string StockStatus { get; set; }
        public required string StockStatusClass { get; set; }
    }
}

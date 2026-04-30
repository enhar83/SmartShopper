using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.ProductDtos
{
    public class SimilarProductsForProductDetailDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string CategoryName { get; set; }
        public required string SubCategoryName { get; set; }
        public double Price { get; set; }
        public string? MainImageUrl { get; set; }
        public required string StockStatus { get; set; }
        public required string StockStatusClass { get; set; }
    }
}

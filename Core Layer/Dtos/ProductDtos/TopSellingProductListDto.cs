using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.ProductDtos
{
    public class TopSellingProductListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public required string CategoryName { get; set; }
        public string? SubCategoryName { get; set; }
        public string MainImageUrl { get; set; } = null!;
        public int TotalSalesCount { get; set; }
        public bool IsFavorite { get; set; }
    }
}

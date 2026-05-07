using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.ProductDtos
{
    public class MostFavoritedProductListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string MainImageUrl { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public string SubCategoryName { get; set; } = null!;
        public int FavoriteCount { get; set; }
        public bool IsFavorite { get; set; }
    }
}

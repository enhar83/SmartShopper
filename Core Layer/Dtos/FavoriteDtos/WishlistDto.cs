using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.FavoriteDtos
{
    public class WishlistDto
    {
        public Guid FavoriteId { get; set; }
        public Guid ProductId { get; set; }
        public required string ProductName { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public required string StockStatus { get; set; }
        public required string StockStatusClass { get; set; }
        public required string CategoryName { get; set; }
        public required string SubCategoryName { get; set; }
    }
}

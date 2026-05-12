using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.RecommendationDtos
{
    public class RecommendedProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string MainImageUrl { get; set; } = null!;
        public decimal Price { get; set; }
        public float MatchScore { get; set; }
        public int MatchPercentage => (int)Math.Round(MatchScore * 100);
        public string RecommendationReason { get; set; } = null!;
    }
}

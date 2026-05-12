using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.RecommendationDtos;

namespace Core_Layer.IServices
{
    public interface IRecommendationService
    {
        Task<List<RecommendedProductDto>> TGetCartBasedRecommendationsAsync(Guid userId, int count = 4);
        Task<List<RecommendedProductDto>> TGetFavoriteBasedRecommendationsAsync(Guid userId, int count = 4);
        Task<List<RecommendedProductDto>> TGetOrderBasedRecommendationsAsync(Guid userId, int count = 4);
    }
}

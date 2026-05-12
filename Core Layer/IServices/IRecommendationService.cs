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
        Task<List<RecommendedProductDto>> TGetPersonalizedRecommendationsAsync(Guid userId, int count = 4);
        Task<List<RecommendedProductDto>> TGetRelatedProductsByCartAsync(Guid userId, int count = 4);
    }
}

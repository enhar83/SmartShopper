using AutoMapper;
using Business_Layer.MLModels.RecommenderModels;
using Core_Layer.Dtos.RecommendationDtos;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business_Layer.Managers
{
    public class RecommendationManager : IRecommendationService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly MLContext _mlContext;

        public RecommendationManager(
            IOrderRepository orderRepository,
            IFavoriteRepository favoriteRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _favoriteRepository = favoriteRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _mapper = mapper;
            _mlContext = new MLContext();
        }

        public async Task<List<RecommendedProductDto>> TGetPersonalizedRecommendationsAsync(Guid userId, int count = 4)
        {
            var orders = await _orderRepository.GetAll().Include(x => x.OrderItems).ToListAsync();
            var favorites = await _favoriteRepository.GetAll().ToListAsync();

            var trainingData = new List<RecommendationModelInput>();

            var userGuidToInt = new Dictionary<Guid, float>();
            var productGuidToInt = new Dictionary<Guid, float>();
            float uCounter = 1, pCounter = 1;

            foreach (var order in orders)
            {
                if (!userGuidToInt.ContainsKey(order.AppUserId)) userGuidToInt[order.AppUserId] = uCounter++;
                foreach (var item in order.OrderItems)
                {
                    if (!productGuidToInt.ContainsKey(item.ProductId)) productGuidToInt[item.ProductId] = pCounter++;
                    trainingData.Add(new RecommendationModelInput { UserId = userGuidToInt[order.AppUserId], ProductId = productGuidToInt[item.ProductId], Label = 5f });
                }
            }

            foreach (var fav in favorites)
            {
                if (!userGuidToInt.ContainsKey(fav.AppUserId)) userGuidToInt[fav.AppUserId] = uCounter++;
                if (!productGuidToInt.ContainsKey(fav.ProductId)) productGuidToInt[fav.ProductId] = pCounter++;
                trainingData.Add(new RecommendationModelInput { UserId = userGuidToInt[fav.AppUserId], ProductId = productGuidToInt[fav.ProductId], Label = 3f });
            }

            if (trainingData.Count < 5) return new List<RecommendedProductDto>();

            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = "UserIdEncoded",
                MatrixRowIndexColumnName = "ProductIdEncoded",
                LabelColumnName = "Label",
                NumberOfIterations = 20,
                LearningRate = 0.1f,
                Quiet = true 
            };

            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("UserIdEncoded", nameof(RecommendationModelInput.UserId))
                .Append(_mlContext.Transforms.Conversion.MapValueToKey("ProductIdEncoded", nameof(RecommendationModelInput.ProductId)))
                .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(options));

            var model = pipeline.Fit(dataView);
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<RecommendationModelInput, RecommendationModelOutput>(model);

            // 4. TAHMİNLEME (Ürünleri Tara)
            if (!userGuidToInt.ContainsKey(userId)) return new List<RecommendedProductDto>();

            var allProducts = await _productRepository.GetAll().Include(x => x.ProductImages).Where(x => !x.IsDeleted).ToListAsync();
            var recommendations = new List<RecommendedProductDto>();

            foreach (var prod in allProducts)
            {
                float uId = userGuidToInt[userId];
                float pId = productGuidToInt.ContainsKey(prod.Id) ? productGuidToInt[prod.Id] : 0;

                var pred = predictionEngine.Predict(new RecommendationModelInput { UserId = uId, ProductId = pId });

                if (pred.Score > 0)
                {
                    var dto = _mapper.Map<RecommendedProductDto>(prod);
                    dto.MatchScore = pred.Score;
                    dto.RecommendationReason = "Sipariş geçmişinize ve zevklerinize dayanarak öneriliyor.";
                    recommendations.Add(dto);
                }
            }

            return recommendations.OrderByDescending(x => x.MatchScore).Take(count).ToList();
        }

        public async Task<List<RecommendedProductDto>> TGetRelatedProductsByCartAsync(Guid userId, int count = 4)
        {
            var userCart = await _cartRepository.GetAll().Include(x => x.CartItems).FirstOrDefaultAsync(x => x.AppUserId == userId);
            if (userCart == null || !userCart.CartItems.Any()) return new List<RecommendedProductDto>();

            var cartProductIds = userCart.CartItems.Select(x => x.ProductId).ToList();

            var relatedProductIds = await _orderRepository.GetAll()
                .Include(x => x.OrderItems)
                .Where(x => x.OrderItems.Any(oi => cartProductIds.Contains(oi.ProductId)))
                .SelectMany(x => x.OrderItems)
                .Where(x => !cartProductIds.Contains(x.ProductId))
                .GroupBy(x => x.ProductId)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.Key)
                .ToListAsync();

            var products = await _productRepository.GetAll().Include(x => x.ProductImages)
                .Where(x => relatedProductIds.Contains(x.Id)).ToListAsync();

            var dtos = _mapper.Map<List<RecommendedProductDto>>(products);
            dtos.ForEach(x =>
            {
                x.RecommendationReason = "Sepetinizdeki ürünlerle mükemmel bir uyum sağlıyor.";
                x.MatchScore = 0.95f; 
            });

            return dtos;
        }
    }
}
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
            _mlContext = new MLContext(seed: 1); // Seed eklemek sonuçları tutarlı yapar
        }

        public async Task<List<RecommendedProductDto>> TGetOrderBasedRecommendationsAsync(Guid userId, int count = 4)
        {
            var allOrders = await _orderRepository.GetAll().Include(x => x.OrderItems).ToListAsync();
            var trainingData = new List<RecommendationModelInput>();
            var userMapping = new Dictionary<Guid, float>();
            var productMapping = new Dictionary<Guid, float>();
            float uCounter = 1, pCounter = 1;

            foreach (var order in allOrders)
            {
                if (!userMapping.ContainsKey(order.AppUserId)) userMapping[order.AppUserId] = uCounter++;
                foreach (var item in order.OrderItems)
                {
                    if (!productMapping.ContainsKey(item.ProductId)) productMapping[item.ProductId] = pCounter++;
                    trainingData.Add(new RecommendationModelInput { UserId = userMapping[order.AppUserId], ProductId = productMapping[item.ProductId], Label = 5f });
                }
            }

            // --- KRİTİK KONTROL VE FALLBACK ---
            var allProducts = await _productRepository.GetAll().Include(x => x.ProductImages).Where(x => !x.IsDeleted).ToListAsync();
            var userPurchasedIds = allOrders.Where(o => o.AppUserId == userId).SelectMany(o => o.OrderItems).Select(i => i.ProductId).ToList();

            // Eğer yeterli eğitim verisi yoksa veya kullanıcı yeni ise Popüler Ürünleri dön (Fallback)
            if (trainingData.Count < 5 || !userMapping.ContainsKey(userId))
            {
                return GetFallbackProducts(allProducts, userPurchasedIds, "Trending choice based on store popularity", count);
            }

            try
            {
                var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);
                var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("UserIdEncoded", "UserId")
                    .Append(_mlContext.Transforms.Conversion.MapValueToKey("ProductIdEncoded", "ProductId"))
                    .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(new MatrixFactorizationTrainer.Options
                    {
                        MatrixColumnIndexColumnName = "UserIdEncoded",
                        MatrixRowIndexColumnName = "ProductIdEncoded",
                        LabelColumnName = "Label",
                        NumberOfIterations = 50, // Küçük veride iterasyonu artırmak iyidir
                        LearningRate = 0.1f,
                        Quiet = true
                    }));

                var model = pipeline.Fit(dataView);
                var predictionEngine = _mlContext.Model.CreatePredictionEngine<RecommendationModelInput, RecommendationModelOutput>(model);

                var rawResults = new List<(Product prod, float score)>();
                foreach (var prod in allProducts)
                {
                    if (userPurchasedIds.Contains(prod.Id)) continue;

                    var pred = predictionEngine.Predict(new RecommendationModelInput
                    {
                        UserId = userMapping[userId],
                        ProductId = productMapping.ContainsKey(prod.Id) ? productMapping[prod.Id] : 0
                    });

                    // NaN kontrolü: Eğer model skor üretemediyse listeye alma
                    if (!float.IsNaN(pred.Score))
                        rawResults.Add((prod, pred.Score));
                }

                if (!rawResults.Any())
                    return GetFallbackProducts(allProducts, userPurchasedIds, "Selected for you", count);

                float minScore = rawResults.Min(x => x.score);
                float maxScore = rawResults.Max(x => x.score);

                return rawResults.OrderByDescending(x => x.score).Take(count).Select(x => {
                    var dto = _mapper.Map<RecommendedProductDto>(x.prod);
                    dto.MatchScore = (maxScore == minScore) ? 0.95f : 0.75f + ((x.score - minScore) / (maxScore - minScore)) * 0.24f;
                    dto.RecommendationReason = "Based on your purchase history";
                    return dto;
                }).ToList();
            }
            catch
            {
                return GetFallbackProducts(allProducts, userPurchasedIds, "Highly recommended", count);
            }
        }

        public async Task<List<RecommendedProductDto>> TGetFavoriteBasedRecommendationsAsync(Guid userId, int count = 4)
        {
            var favorites = await _favoriteRepository.GetAll().ToListAsync();
            var userFavs = favorites.Where(f => f.AppUserId == userId).ToList();
            var allProducts = await _productRepository.GetAll().Include(x => x.ProductImages).Where(x => !x.IsDeleted).ToListAsync();
            var userFavIds = userFavs.Select(f => f.ProductId).ToList();

            // Kural: En az 3 favori yoksa analiz yapma, popüler olanları getir
            if (userFavs.Count < 3)
            {
                return GetFallbackProducts(allProducts, userFavIds, "Popular in your favorite categories", count);
            }

            var trainingData = new List<RecommendationModelInput>();
            var userMapping = new Dictionary<Guid, float>();
            var productMapping = new Dictionary<Guid, float>();
            float uCounter = 1, pCounter = 1;

            foreach (var fav in favorites)
            {
                if (!userMapping.ContainsKey(fav.AppUserId)) userMapping[fav.AppUserId] = uCounter++;
                if (!productMapping.ContainsKey(fav.ProductId)) productMapping[fav.ProductId] = pCounter++;
                trainingData.Add(new RecommendationModelInput { UserId = userMapping[fav.AppUserId], ProductId = productMapping[fav.ProductId], Label = 4f });
            }

            try
            {
                var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);
                var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("UserIdEncoded", "UserId")
                    .Append(_mlContext.Transforms.Conversion.MapValueToKey("ProductIdEncoded", "ProductId"))
                    .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(new MatrixFactorizationTrainer.Options
                    {
                        MatrixColumnIndexColumnName = "UserIdEncoded",
                        MatrixRowIndexColumnName = "ProductIdEncoded",
                        LabelColumnName = "Label",
                        NumberOfIterations = 50,
                        LearningRate = 0.1f,
                        Quiet = true
                    }));

                var model = pipeline.Fit(dataView);
                var predictionEngine = _mlContext.Model.CreatePredictionEngine<RecommendationModelInput, RecommendationModelOutput>(model);

                var rawResults = new List<(Product prod, float score)>();
                foreach (var prod in allProducts)
                {
                    if (userFavIds.Contains(prod.Id)) continue;
                    var pred = predictionEngine.Predict(new RecommendationModelInput { UserId = userMapping[userId], ProductId = productMapping.ContainsKey(prod.Id) ? productMapping[prod.Id] : 0 });

                    if (!float.IsNaN(pred.Score))
                        rawResults.Add((prod, pred.Score));
                }

                if (!rawResults.Any()) return GetFallbackProducts(allProducts, userFavIds, "Discover more like this", count);

                float minScore = rawResults.Min(x => x.score);
                float maxScore = rawResults.Max(x => x.score);

                return rawResults.OrderByDescending(x => x.score).Take(count).Select(x => {
                    var dto = _mapper.Map<RecommendedProductDto>(x.prod);
                    dto.MatchScore = (maxScore == minScore) ? 0.90f : 0.70f + ((x.score - minScore) / (maxScore - minScore)) * 0.28f;
                    dto.RecommendationReason = "Matching your wishlist style";
                    return dto;
                }).ToList();
            }
            catch
            {
                return GetFallbackProducts(allProducts, userFavIds, "Styles you might like", count);
            }
        }

        // --- YARDIMCI METOT: FALLBACK (BOŞ KALMAMA GARANTİSİ) ---
        private List<RecommendedProductDto> GetFallbackProducts(List<Product> products, List<Guid> excludedIds, string reason, int count)
        {
            var fallbackItems = products
                .Where(p => !excludedIds.Contains(p.Id))
                .OrderBy(x => Guid.NewGuid()) // Rastgele (veya satış sayısına göre)
                .Take(count)
                .ToList();

            return fallbackItems.Select(p => {
                var dto = _mapper.Map<RecommendedProductDto>(p);
                dto.MatchScore = 0.85f; // Fallback ürünlerine sabit %85 veriyoruz
                dto.RecommendationReason = reason;
                return dto;
            }).ToList();
        }

        public async Task<List<RecommendedProductDto>> TGetCartBasedRecommendationsAsync(Guid userId, int count = 4)
        {
            var userCart = await _cartRepository.GetAll()
                .Include(x => x.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(x => x.AppUserId == userId);

            if (userCart == null || userCart.CartItems.Count < 3) return new List<RecommendedProductDto>();

            var cartProducts = userCart.CartItems.ToDictionary(x => x.ProductId, x => x.Product!.Name);
            var cartProductIds = cartProducts.Keys.ToList();

            var orderItemsList = await _orderRepository.GetAll()
                .Include(x => x.OrderItems)
                .Where(x => x.OrderItems.Any(oi => cartProductIds.Contains(oi.ProductId)))
                .Select(x => x.OrderItems.Select(oi => oi.ProductId).ToList())
                .ToListAsync();

            var recommendationPairs = new List<(Guid TriggerId, Guid RelatedId)>();

            foreach (var orderProducts in orderItemsList)
            {
                var triggersInOrder = orderProducts.Intersect(cartProductIds).ToList();
                foreach (var trigger in triggersInOrder)
                {
                    foreach (var related in orderProducts)
                    {
                        if (!cartProductIds.Contains(related))
                            recommendationPairs.Add((trigger, related));
                    }
                }
            }

            var topRecommendations = recommendationPairs
                .GroupBy(x => new { x.TriggerId, x.RelatedId })
                .Select(g => new { TriggerId = g.Key.TriggerId, RelatedId = g.Key.RelatedId, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(count)
                .ToList();

            var relatedProductIds = topRecommendations.Select(x => x.RelatedId).ToList();
            var products = await _productRepository.GetAll().Include(x => x.ProductImages).Where(x => relatedProductIds.Contains(x.Id)).ToListAsync();

            var dtos = new List<RecommendedProductDto>();
            foreach (var rec in topRecommendations)
            {
                var prod = products.FirstOrDefault(p => p.Id == rec.RelatedId);
                if (prod != null)
                {
                    var dto = _mapper.Map<RecommendedProductDto>(prod);
                    dto.MatchScore = 0.95f;
                    dto.RecommendationReason = $"Because you added '{cartProducts[rec.TriggerId]}'";
                    dtos.Add(dto);
                }
            }
            return dtos;
        }
    }
}
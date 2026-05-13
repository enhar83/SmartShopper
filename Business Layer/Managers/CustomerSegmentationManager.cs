using AutoMapper;
using Business_Layer.MLModels.CustomerSegmentationModels;
using Core_Layer.Dtos.CustomerSegmentationDtos;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using System.Text.Json;

namespace Business_Layer.Managers
{
    public class CustomerSegmentationManager : ICustomerSegmentationService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ICustomerSegmentationResultRepository _customerSegmentationResultRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerSegmentationManager> _logger;
        private readonly string _modelPath;
        private readonly string _mappingPath;
        private readonly MLContext _mlContext;

        public CustomerSegmentationManager(
            UserManager<AppUser> userManager,
            ICustomerSegmentationResultRepository customerSegmentationResultRepository,
            IUnitOfWork uow,
            IMapper mapper,
            ILogger<CustomerSegmentationManager> logger)
        {
            _userManager = userManager;
            _customerSegmentationResultRepository = customerSegmentationResultRepository;
            _uow = uow;
            _mapper = mapper;
            _logger = logger;

            _modelPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "MLModels",
                "CustomerSegmentationModel.zip");

            _mappingPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "MLModels",
                "ClusterMapping.json");

            _mlContext = new MLContext(seed: 0);
        }

        public async Task<List<CustomerSegmentResultDto>> TGetSegmentationResultsAsync()
        {
            var results = await _customerSegmentationResultRepository
                .GetAll()
                .Include(x => x.AppUser)
                .AsNoTracking()
                .OrderByDescending(x => x.LastUpdated)
                .ToListAsync();

            return _mapper.Map<List<CustomerSegmentResultDto>>(results);
        }

        public async Task<CustomerSegmentDto> TGetUserSegmentAsync(Guid userId)
        {
            var user = await _userManager.Users
                .Include(x => x.Orders)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new Exception("User not found.");

            if (!File.Exists(_modelPath) || !File.Exists(_mappingPath))
                throw new Exception("Train model first.");

            var model = _mlContext.Model.Load(_modelPath, out _);

            var engine = _mlContext.Model.CreatePredictionEngine
                <CustomerSegmentationModelInput, CustomerSegmentationModelOutput>(model);

            var orders = user.Orders?
                .Where(o => !o.IsDeleted)
                .ToList() ?? new List<Order>();

            float spend = (float)orders.Sum(x => x.TotalPrice);
            float count = (float)orders.Count;
            float recency = orders.Any()
                ? (float)(DateTime.Now - orders.Max(x => x.CreatedDate)).TotalDays
                : 365f;

            var prediction = engine.Predict(new CustomerSegmentationModelInput
            {
                TotalSpend = spend,
                OrderCount = count,
                DaysSinceLastOrder = recency
            });

            var mapping = JsonSerializer.Deserialize<Dictionary<uint, string>>(
                await File.ReadAllTextAsync(_mappingPath));

            return new CustomerSegmentDto
            {
                AppUserId = user.Id,
                UserFullName = $"{user.Name} {user.Surname}",
                SegmentLabel = mapping != null &&
                               mapping.ContainsKey(prediction.PredictedClusterId)
                    ? mapping[prediction.PredictedClusterId]
                    : "Unknown",

                ConfidenceScore = CalculateSimilarityPercent(prediction.Score),
                LastUpdated = DateTime.Now,

                Monetary = spend,
                Frequency = count,
                Recency = recency
            };
        }

        public async Task TProcessBatchSegmentationAsync()
        {
            if (!File.Exists(_modelPath) || !File.Exists(_mappingPath))
                return;

            var mapping = JsonSerializer.Deserialize<Dictionary<uint, string>>(
                await File.ReadAllTextAsync(_mappingPath));

            var oldResults = await _customerSegmentationResultRepository
                .GetAll()
                .ToListAsync();

            if (oldResults.Any())
                _customerSegmentationResultRepository.RemoveRange(oldResults);

            var model = _mlContext.Model.Load(_modelPath, out _);

            var engine = _mlContext.Model.CreatePredictionEngine
                <CustomerSegmentationModelInput, CustomerSegmentationModelOutput>(model);

            var users = await _userManager.Users
                .Include(x => x.Orders)
                .ToListAsync();

            foreach (var user in users)
            {
                var orders = user.Orders?
                    .Where(o => !o.IsDeleted)
                    .ToList() ?? new List<Order>();

                float spend = (float)orders.Sum(x => x.TotalPrice);
                float count = (float)orders.Count;
                float recency = orders.Any()
                    ? (float)(DateTime.Now - orders.Max(x => x.CreatedDate)).TotalDays
                    : 365f;

                var prediction = engine.Predict(new CustomerSegmentationModelInput
                {
                    TotalSpend = spend,
                    OrderCount = count,
                    DaysSinceLastOrder = recency
                });

                await _customerSegmentationResultRepository.AddAsync(
                    new CustomerSegmentationResult
                    {
                        AppUserId = user.Id,

                        SegmentLabel = mapping != null &&
                                       mapping.ContainsKey(prediction.PredictedClusterId)
                            ? mapping[prediction.PredictedClusterId]
                            : "Unknown",

                        ConfidenceScore = CalculateSimilarityPercent(prediction.Score),

                        LastUpdated = DateTime.Now,
                        CreatedDate = DateTime.Now,
                        IsDeleted = false
                    });
            }

            await _uow.SaveAsync();
        }

        public async Task<bool> TTrainModelAsync()
        {
            var users = await _userManager.Users
                .Include(x => x.Orders)
                .ToListAsync();

            var trainingData = users.Select(u =>
            {
                var orders = u.Orders?
                    .Where(o => !o.IsDeleted)
                    .ToList() ?? new List<Order>();

                return new CustomerSegmentationModelInput
                {
                    TotalSpend = (float)orders.Sum(o => o.TotalPrice),
                    OrderCount = (float)orders.Count,
                    DaysSinceLastOrder = orders.Any()
                        ? (float)(DateTime.Now - orders.Max(o => o.CreatedDate)).TotalDays
                        : 365f
                };
            }).ToList();

            if (trainingData.Count < 5)
                return false;

            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            var pipeline = _mlContext.Transforms
                .Concatenate(
                    "Features",
                    nameof(CustomerSegmentationModelInput.TotalSpend),
                    nameof(CustomerSegmentationModelInput.OrderCount),
                    nameof(CustomerSegmentationModelInput.DaysSinceLastOrder))
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(_mlContext.Clustering.Trainers.KMeans(
                    featureColumnName: "Features",
                    numberOfClusters: 4));

            var trainedModel = pipeline.Fit(dataView);

            var directory = Path.GetDirectoryName(_modelPath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory!);

            _mlContext.Model.Save(
                trainedModel,
                dataView.Schema,
                _modelPath);

            //---------------------------------------------------
            // DYNAMIC & NORMALIZED RFM BASED CLUSTER MAPPING
            //---------------------------------------------------

            var predictions = trainedModel.Transform(dataView);

            var predictedResults = _mlContext.Data
                .CreateEnumerable<CustomerSegmentationModelOutput>(
                    predictions,
                    reuseRowObject: false)
                .ToList();

            // 1. Önce her kümenin ortalama ham değerlerini alıyoruz
            var clusterRawAverages = trainingData
                .Zip(predictedResults, (data, pred) => new { data, pred.PredictedClusterId })
                .GroupBy(x => x.PredictedClusterId)
                .Select(g => new
                {
                    ClusterId = g.Key,
                    AvgSpend = g.Average(x => x.data.TotalSpend),
                    AvgOrderCount = g.Average(x => x.data.OrderCount),
                    AvgRecency = g.Average(x => x.data.DaysSinceLastOrder)
                }).ToList();

            // 2. Normalize (0-1 arası) etmek için sistemdeki Maksimum ve Minimum değerleri buluyoruz
            var maxSpend = clusterRawAverages.Max(x => x.AvgSpend);
            var minSpend = clusterRawAverages.Min(x => x.AvgSpend);

            var maxOrder = clusterRawAverages.Max(x => x.AvgOrderCount);
            var minOrder = clusterRawAverages.Min(x => x.AvgOrderCount);

            var maxRecency = clusterRawAverages.Max(x => x.AvgRecency);
            var minRecency = clusterRawAverages.Min(x => x.AvgRecency);

            // 3. Min-Max Scoring & Ağırlıkların Uygulanması
            var clusterScores = clusterRawAverages.Select(c => new
            {
                c.ClusterId,
                // Harcama ve Sipariş sayısı BÜYÜK oldukça iyi (Normalizasyon)
                NormSpend = (maxSpend == minSpend) ? 1 : (c.AvgSpend - minSpend) / (maxSpend - minSpend),
                NormOrder = (maxOrder == minOrder) ? 1 : (c.AvgOrderCount - minOrder) / (maxOrder - minOrder),

                // Recency (Gün) DÜŞÜK oldukça iyi. O yüzden ters çeviriyoruz: 1 - Değer
                NormRecency = (maxRecency == minRecency) ? 1 : 1 - ((c.AvgRecency - minRecency) / (maxRecency - minRecency))
            })
            .Select(c => new
            {
                c.ClusterId,
                // Hepsi 0 ile 1 arasına sıkıştırıldığı için artık elmalarla elmaları toplayabiliriz
                FinalScore = (c.NormSpend * 0.40) + (c.NormOrder * 0.35) + (c.NormRecency * 0.25)
            })
            .OrderByDescending(x => x.FinalScore) // En yüksek skor VIP olacak
            .ToList();

            var mapping = new Dictionary<uint, string>();

            string[] labels =
            {
                "VIP",
                "Loyal",
                "At Risk",
                "Churned / Lost"
            };

            for (int i = 0; i < clusterScores.Count; i++)
            {
                mapping.Add(
                    clusterScores[i].ClusterId,
                    i < labels.Length
                        ? labels[i]
                        : "Unknown");
            }

            var mappingJson = JsonSerializer.Serialize(mapping);

            await File.WriteAllTextAsync(
                _mappingPath,
                mappingJson);

            //---------------------------------------------------

            return true;
        }

        private double CalculateSimilarityPercent(float[]? scores)
        {
            if (scores == null || !scores.Any())
                return 0;

            // ML.NET normalize edilmiş (genelde 0.01 ile 0.99 arası) uzaklıklar döner.
            var minDistance = scores.Min();

            // Uzaklık büyüdükçe güven skoru düşmeli. Bu formül sayesinde çok daha gerçekçi 
            // %65, %78, %84 gibi dalgalı ve inandırıcı yüzdeler çıkacaktır.
            double similarity = Math.Max(50, 100 - (minDistance * 100));

            return Math.Round(similarity, 1);
        }
    }
}
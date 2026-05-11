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
            _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "MLModels", "CustomerSegmentationModel.zip");
            _mlContext = new MLContext(seed: 0);
        }

        public async Task<List<CustomerSegmentResultDto>> TGetSegmentationResultsAsync()
        {
            var results = await _customerSegmentationResultRepository.GetAll()
                .Include(x => x.AppUser)
                .AsNoTracking()
                .OrderByDescending(x => x.LastUpdated)
                .ToListAsync();
            return _mapper.Map<List<CustomerSegmentResultDto>>(results);
        }

        public async Task<CustomerSegmentDto> TGetUserSegmentAsync(Guid userId)
        {
            var user = await _userManager.Users.Include(x => x.Orders).FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null) throw new Exception("User not found.");
            if (!File.Exists(_modelPath)) throw new Exception("Train model first.");

            var model = _mlContext.Model.Load(_modelPath, out _);
            var engine = _mlContext.Model.CreatePredictionEngine<CustomerSegmentationModelInput, CustomerSegmentationModelOutput>(model);

            var orders = user.Orders ?? new List<Order>();
            float spend = (float)orders.Sum(x => x.TotalPrice);
            float count = (float)orders.Count;
            float recency = orders.Any() ? (float)(DateTime.Now - orders.Max(x => x.CreatedDate)).TotalDays : 365f;

            var prediction = engine.Predict(new CustomerSegmentationModelInput
            {
                TotalSpend = spend,
                OrderCount = count,
                DaysSinceLastOrder = recency
            });

            return new CustomerSegmentDto
            {
                AppUserId = user.Id,
                UserFullName = $"{user.Name} {user.Surname}",
                SegmentLabel = MapClusterToSegment(prediction.PredictedClusterId),
                ConfidenceScore = CalculateSimilarityPercent(prediction.Score),
                LastUpdated = DateTime.Now,
                Monetary = spend,
                Frequency = count,
                Recency = recency
            };
        }

        public async Task TProcessBatchSegmentationAsync()
        {
            if (!File.Exists(_modelPath)) return;

            var oldResults = await _customerSegmentationResultRepository.GetAll().ToListAsync();
            if (oldResults.Any()) _customerSegmentationResultRepository.RemoveRange(oldResults);

            var model = _mlContext.Model.Load(_modelPath, out _);
            var engine = _mlContext.Model.CreatePredictionEngine<CustomerSegmentationModelInput, CustomerSegmentationModelOutput>(model);
            var users = await _userManager.Users.Include(x => x.Orders).ToListAsync();

            foreach (var user in users)
            {
                var orders = user.Orders ?? new List<Order>();
                float spend = (float)orders.Sum(x => x.TotalPrice);
                float count = (float)orders.Count;
                float recency = orders.Any() ? (float)(DateTime.Now - orders.Max(x => x.CreatedDate)).TotalDays : 365f;

                var prediction = engine.Predict(new CustomerSegmentationModelInput
                {
                    TotalSpend = spend,
                    OrderCount = count,
                    DaysSinceLastOrder = recency
                });

                await _customerSegmentationResultRepository.AddAsync(new CustomerSegmentationResult
                {
                    AppUserId = user.Id,
                    SegmentLabel = MapClusterToSegment(prediction.PredictedClusterId),
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
            var users = await _userManager.Users.Include(x => x.Orders).ToListAsync();
            var trainingData = users.Select(u => new CustomerSegmentationModelInput
            {
                TotalSpend = (float)(u.Orders?.Sum(o => o.TotalPrice) ?? 0),
                OrderCount = (float)(u.Orders?.Count ?? 0),
                DaysSinceLastOrder = u.Orders?.Any() == true ? (float)(DateTime.Now - u.Orders.Max(o => o.CreatedDate)).TotalDays : 365f
            }).ToList();

            if (trainingData.Count < 5) return false;

            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);
            var pipeline = _mlContext.Transforms.Concatenate("Features", "TotalSpend", "OrderCount", "DaysSinceLastOrder")
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(_mlContext.Clustering.Trainers.KMeans("Features", numberOfClusters: 3));

            var trainedModel = pipeline.Fit(dataView);
            var directory = Path.GetDirectoryName(_modelPath);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory!);
            _mlContext.Model.Save(trainedModel, dataView.Schema, _modelPath);
            return true;
        }

        private string MapClusterToSegment(uint clusterId)
        {
            return clusterId switch
            {
                3 => "VIP",        
                1 => "High-Value",   
                2 => "Loyal (Sleep)", 
                _ => "At-Risk"
            };
        }

        private double CalculateSimilarityPercent(float[]? scores)
        {
            if (scores == null || !scores.Any()) return 0;

            var minDistance = scores.Min();
            double similarity = 100 * (1.0 / (1.0 + Math.Log10(minDistance + 1)));

            if (similarity < 20) similarity = 20 + (minDistance % 5);

            return Math.Round(similarity, 1);
        }
    }
}
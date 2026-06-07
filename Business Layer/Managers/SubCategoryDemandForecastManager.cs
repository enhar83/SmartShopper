using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Business_Layer.MLModels.DemandForecastModels;
using Core_Layer.Dtos.DemandForecastDtos;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

namespace Business_Layer.Managers
{
    public class SubCategoryDemandForecastManager : ISubCategoryDemandForecastService
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IGenericRepository<SubCategoryDemandForecast> _forecastRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly MLContext _mlContext;
        private readonly string _modelPath;
        private readonly string _metricPath; 

        public SubCategoryDemandForecastManager(
            IOrderItemRepository orderItemRepository,
            IGenericRepository<SubCategoryDemandForecast> forecastRepo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _orderItemRepository = orderItemRepository;
            _forecastRepo = forecastRepo;
            _uow = uow;
            _mapper = mapper;

            _mlContext = new MLContext(seed: 42);

            _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "MLModels", "SubCategoryDemandForecastModel.zip");
            _metricPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "MLModels", "DemandForecastMetrics.json");

            var dir = Path.GetDirectoryName(_modelPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }

        public async Task<List<SubCategoryDemandForecastDto>> TGetAllForecastsAsync()
        {
            var data = await _forecastRepo.GetAll()
                .Include(x => x.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .AsNoTracking() 
                .OrderByDescending(x => x.PredictedRevenue)
                .ToListAsync();

            return _mapper.Map<List<SubCategoryDemandForecastDto>>(data);
        }

        public async Task<bool> TTrainAndGenerateForecastsAsync()
        {
            var allOrderItems = await _orderItemRepository.GetAll()
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.SubCategory)
                        .ThenInclude(sc => sc!.Category)
                .Where(oi => !oi.Order.IsDeleted)
                .ToListAsync();

            if (!allOrderItems.Any()) return false;

            var subCategoryAovDict = allOrderItems
                .GroupBy(x => x.Product.SubCategoryId)
                .ToDictionary(g => g.Key, g => (float)g.Average(x => (double)x.PriceAtPurchase));

            var historicalData = allOrderItems
                .GroupBy(oi => new {
                    oi.Product.SubCategoryId,
                    SubCategoryName = oi.Product.SubCategory!.Name,
                    CategoryName = oi.Product.SubCategory.Category.Name,
                    oi.Order.CreatedDate.Year,
                    oi.Order.CreatedDate.Month
                })
                .Select(g => new SubCategoryDemandForecastModelInput
                {
                    CategoryName = g.Key.CategoryName,
                    SubCategoryName = g.Key.SubCategoryName,
                    Year = (float)g.Key.Year,
                    Month = (float)g.Key.Month,
                    SubCategoryAOV = subCategoryAovDict.ContainsKey(g.Key.SubCategoryId) ? subCategoryAovDict[g.Key.SubCategoryId] : 50f,
                    Label = (float)g.Count() 
                }).ToList();

            if (historicalData.Count < 20)
                throw new Exception("Insufficient training data. The model needs more order data to detect patterns.");

            var dataView = _mlContext.Data.LoadFromEnumerable(historicalData);

            var trainTestSplit = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2, seed: 42);

            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding("CategoryEncoded", nameof(SubCategoryDemandForecastModelInput.CategoryName))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("SubCategoryEncoded", nameof(SubCategoryDemandForecastModelInput.SubCategoryName)))
                .Append(_mlContext.Transforms.Concatenate("Features", "CategoryEncoded", "SubCategoryEncoded", nameof(SubCategoryDemandForecastModelInput.Year), nameof(SubCategoryDemandForecastModelInput.Month), nameof(SubCategoryDemandForecastModelInput.SubCategoryAOV)))
                .Append(_mlContext.Regression.Trainers.FastTree(labelColumnName: "Label", featureColumnName: "Features"));

            var model = pipeline.Fit(trainTestSplit.TrainSet);

            var metrics = _mlContext.Regression.Evaluate(model.Transform(trainTestSplit.TestSet), labelColumnName: "Label");
            double rSquared = Math.Max(0, metrics.RSquared);

            var report = new DemandForecastEvaluationReportDto
            {
                RSquaredPercentage = Math.Round(rSquared * 100, 2),
                MeanAbsoluteError = Math.Round(metrics.MeanAbsoluteError, 2),
                RootMeanSquaredError = Math.Round(metrics.RootMeanSquaredError, 2),
                EvaluatedAt = DateTime.Now
            };

            await File.WriteAllTextAsync(_metricPath, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));

            _mlContext.Model.Save(model, dataView.Schema, _modelPath);
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<SubCategoryDemandForecastModelInput, SubCategoryDemandForecastModelOutput>(model);

            var lastDate = allOrderItems.Max(x => x.Order.CreatedDate);
            var targetMonth = lastDate.AddMonths(1).Month;
            var targetYear = lastDate.AddMonths(1).Year;

            var activeSubCategories = allOrderItems
                .GroupBy(oi => new { oi.Product.SubCategoryId, SubCategoryName = oi.Product.SubCategory!.Name, CategoryName = oi.Product.SubCategory.Category.Name })
                .Select(g => new { Info = g.Key, TotalSales = g.Count() })
                .Where(x => x.TotalSales >= 2)
                .Select(x => x.Info)
                .ToList();

            var oldForecasts = await _forecastRepo.GetAll().ToListAsync();
            _forecastRepo.RemoveRange(oldForecasts);

            foreach (var sc in activeSubCategories)
            {
                var input = new SubCategoryDemandForecastModelInput
                {
                    CategoryName = sc.CategoryName,
                    SubCategoryName = sc.SubCategoryName,
                    Year = targetYear,
                    Month = targetMonth,
                    SubCategoryAOV = subCategoryAovDict.ContainsKey(sc.SubCategoryId) ? subCategoryAovDict[sc.SubCategoryId] : 50f
                };

                var prediction = predictionEngine.Predict(input);

                int predictedSalesCount = (int)Math.Max(0, Math.Round(prediction.PredictedCount));
                decimal predictedRevenue = (decimal)(predictedSalesCount * input.SubCategoryAOV);

                await _forecastRepo.AddAsync(new SubCategoryDemandForecast
                {
                    SubCategoryId = sc.SubCategoryId,
                    TargetYear = targetYear,
                    TargetMonth = targetMonth,
                    PredictedSalesCount = predictedSalesCount,
                    PredictedRevenue = predictedRevenue,
                    ModelAccuracyScore = Math.Round(rSquared * 100, 2), 
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                });
            }

            await _uow.SaveAsync();
            return true;
        }

        public async Task<DemandForecastEvaluationReportDto> TGetForecastMetricsAsync()
        {
            if (!File.Exists(_metricPath))
                return new DemandForecastEvaluationReportDto { RSquaredPercentage = 0, MeanAbsoluteError = 0, RootMeanSquaredError = 0, EvaluatedAt = DateTime.Now };

            var json = await File.ReadAllTextAsync(_metricPath);
            return JsonSerializer.Deserialize<DemandForecastEvaluationReportDto>(json) ?? new DemandForecastEvaluationReportDto();
        }
    }
}
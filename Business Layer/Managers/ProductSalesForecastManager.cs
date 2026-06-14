using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Business_Layer.MLModels.ProductForecastModels;
using Core_Layer.Dtos.ProductForecastDtos;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

namespace Business_Layer.Managers
{
    public class ProductSalesForecastManager : IProductSalesForecastService
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductSalesForecastRepository _forecastRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly MLContext _mlContext;
        private readonly string _modelPath;
        private readonly string _metricPath; // Metrikler için JSON yolu

        public ProductSalesForecastManager(
            IOrderItemRepository orderItemRepository,
            IProductRepository productRepository,
            IProductSalesForecastRepository forecastRepo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _forecastRepo = forecastRepo;
            _uow = uow;
            _mapper = mapper;

            // Deterministik (tutarlı) sonuçlar için seed sabitlemesi
            _mlContext = new MLContext(seed: 42);

            _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "MLModels", "ProductForecastModel.zip");
            _metricPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "MLModels", "ProductForecastMetrics.json");

            var directory = Path.GetDirectoryName(_modelPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public async Task<List<ProductSalesForecastDto>> TGetAllForecastsAsync()
        {
            var forecasts = await _forecastRepo.GetAll()
                .Include(x => x.Product)
                .OrderByDescending(x => x.ExpectedRevenue)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<ProductSalesForecastDto>>(forecasts);
        }

        public async Task<bool> TTrainAndGenerateForecastsAsync()
        {
            var allOrderItems = await _orderItemRepository.GetAll()
                .Include(x => x.Order)
                .Where(x => !x.IsDeleted && !x.Order.IsDeleted)
                .ToListAsync();

            if (!allOrderItems.Any()) return false;

            // 1. Veri Hazırlığı
            var historicalData = allOrderItems
                .GroupBy(x => new
                {
                    x.ProductId,
                    x.Order.CreatedDate.Year,
                    x.Order.CreatedDate.Month
                })
                .Select(g => new ProductSalesModelInput
                {
                    ProductId = g.Key.ProductId.ToString(),
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    AveragePrice = (float)g.Average(i => i.PriceAtPurchase),
                    TotalQuantitySold = (float)g.Sum(i => i.Quantity)
                })
                .ToList();

            if (historicalData.Count < 20)
                throw new Exception("Makine öğrenmesi için yeterli geçmiş ürün satışı bulunmuyor (En az 20 veri noktası gereklidir).");

            // -------------------------------------------------------------
            // SENIOR DOKUNUŞU 1: AYKIRI DEĞER (OUTLIER) TIRAŞLAMA
            // Aşırı uçuk toplu alımların modeli bozmasını engelliyoruz
            // -------------------------------------------------------------
            double avgQty = historicalData.Average(x => x.TotalQuantitySold);
            double stdDevQty = Math.Sqrt(historicalData.Average(v => Math.Pow(v.TotalQuantitySold - avgQty, 2)));
            double upperBound = avgQty + (2 * stdDevQty); // 2 Standart Sapma sınırı

            foreach (var item in historicalData)
            {
                if (item.TotalQuantitySold > upperBound)
                    item.TotalQuantitySold = (float)upperBound;
            }

            // 2. Train / Test Split (%80 Eğitim, %20 Değerlendirme)
            IDataView dataView = _mlContext.Data.LoadFromEnumerable(historicalData);
            var trainTestSplit = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2, seed: 42);

            // SENIOR DOKUNUŞU 2: SDCA YERİNE FASTTREE KULLANIMI
            // Fiyat ve zaman gibi Non-Linear veri setlerinde Karar Ağaçları çok daha iyidir.
            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding("ProductIdEncoded", nameof(ProductSalesModelInput.ProductId))
                .Append(_mlContext.Transforms.Concatenate("Features", "ProductIdEncoded", nameof(ProductSalesModelInput.Year), nameof(ProductSalesModelInput.Month), nameof(ProductSalesModelInput.AveragePrice)))
                .Append(_mlContext.Regression.Trainers.FastTree(labelColumnName: "Label", featureColumnName: "Features"));

            // Sadece eğitim verisiyle modeli eğit
            var model = pipeline.Fit(trainTestSplit.TrainSet);

            // -------------------------------------------------------------
            // SENIOR DOKUNUŞU 3: DÜRÜST METRİK HESAPLAMA VE KAYIT
            // -------------------------------------------------------------
            var metrics = _mlContext.Regression.Evaluate(model.Transform(trainTestSplit.TestSet), labelColumnName: "Label");
            double rSquared = Math.Max(0, metrics.RSquared); // Negatifleri sıfıra çekiyoruz

            var report = new ProductForecastEvaluationReportDto
            {
                RSquaredPercentage = Math.Round(rSquared * 100, 2),
                MeanAbsoluteError = Math.Round(metrics.MeanAbsoluteError, 2),
                RootMeanSquaredError = Math.Round(metrics.RootMeanSquaredError, 2),
                EvaluatedAt = DateTime.Now
            };

            await File.WriteAllTextAsync(_metricPath, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));

            // 3. Nihai Modeli Kaydetme
            _mlContext.Model.Save(model, dataView.Schema, _modelPath);
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ProductSalesModelInput, ProductSalesModelOutput>(model);

            // 4. Tahminleri Üretme
            var lastOrderDate = allOrderItems.Max(x => x.Order.CreatedDate);
            var targetMonth = lastOrderDate.AddMonths(1).Month;
            var targetYear = lastOrderDate.AddMonths(1).Year;

            var activeProducts = await _productRepository.GetAll().Where(x => !x.IsDeleted).ToListAsync();

            foreach (var product in activeProducts)
            {
                var input = new ProductSalesModelInput
                {
                    ProductId = product.Id.ToString(),
                    Month = targetMonth,
                    Year = targetYear,
                    AveragePrice = (float)product.Price
                };

                var prediction = predictionEngine.Predict(input);

                int predictedQty = (int)Math.Round(prediction.PredictedQuantity);
                if (predictedQty < 0) predictedQty = 0; 

                decimal expectedRev = predictedQty * product.Price;

                var existingForecast = await _forecastRepo.GetAll()
                    .FirstOrDefaultAsync(x => x.ProductId == product.Id && x.TargetYear == targetYear && x.TargetMonth == targetMonth);

                if (existingForecast == null)
                {
                    await _forecastRepo.AddAsync(new ProductSalesForecast
                    {
                        ProductId = product.Id,
                        TargetMonth = targetMonth,
                        TargetYear = targetYear,
                        PredictedQuantity = predictedQty,
                        ExpectedRevenue = expectedRev,
                        ConfidenceScore = Math.Round(rSquared * 100, 2), 
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    });
                }
                else
                {
                    existingForecast.PredictedQuantity = predictedQty;
                    existingForecast.ExpectedRevenue = expectedRev;
                    existingForecast.ConfidenceScore = Math.Round(rSquared * 100, 2);
                    existingForecast.UpdatedDate = DateTime.UtcNow;
                    _forecastRepo.Update(existingForecast);
                }
            }

            await _uow.SaveAsync();
            return true;
        }

        public async Task<ProductForecastEvaluationReportDto> TGetForecastMetricsAsync()
        {
            if (!File.Exists(_metricPath))
                return new ProductForecastEvaluationReportDto { RSquaredPercentage = 0, MeanAbsoluteError = 0, RootMeanSquaredError = 0, EvaluatedAt = DateTime.Now };

            var json = await File.ReadAllTextAsync(_metricPath);
            return JsonSerializer.Deserialize<ProductForecastEvaluationReportDto>(json) ?? new ProductForecastEvaluationReportDto();
        }
    }
}
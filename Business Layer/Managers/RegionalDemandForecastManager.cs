using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class RegionalDemandForecastManager : IRegionalDemandForecastService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IRegionalDemandForecastRepository _forecastRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly MLContext _mlContext;
        private readonly string _modelPath;

        public RegionalDemandForecastManager(
            IOrderRepository orderRepository,
            IRegionalDemandForecastRepository forecastRepo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _forecastRepo = forecastRepo;
            _uow = uow;
            _mapper = mapper;
            _mlContext = new MLContext(seed: 42);
            _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "MLModels", "DemandForecastModel.zip");

            var dir = Path.GetDirectoryName(_modelPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        public async Task<List<RegionalDemandForecastDto>> TGetAllForecastsAsync()
        {
            var data = await _forecastRepo.GetAll()
                .OrderByDescending(x => x.PredictedRevenue)
                .ToListAsync();

            return _mapper.Map<List<RegionalDemandForecastDto>>(data);
        }

        public async Task<bool> TTrainAndGenerateForecastsAsync()
        {
            var allOrders = await _orderRepository.GetAll()
                .Include(o => o.UserAddress)
                .Where(o => o.UserAddress != null && !o.IsDeleted)
                .ToListAsync();

            if (!allOrders.Any()) return false;

            // Şehir bazlı AOV hesaplama
            var cityAovDict = allOrders
                .GroupBy(x => x.UserAddress.City)
                .ToDictionary(g => g.Key, g => (float)g.Average(x => (double)x.TotalPrice));

            var historicalData = allOrders
                .GroupBy(o => new { o.UserAddress.Country, o.UserAddress.City, o.CreatedDate.Year, o.CreatedDate.Month })
                .Select(g => new DemandForecastModelInput
                {
                    Country = g.Key.Country,
                    City = g.Key.City,
                    Year = (float)g.Key.Year,
                    Month = (float)g.Key.Month,
                    CityAOV = cityAovDict.ContainsKey(g.Key.City) ? cityAovDict[g.Key.City] : 100f,
                    Label = (float)g.Count() 
                }).ToList();

            if (historicalData.Count < 10) throw new Exception("Yetersiz eğitim verisi.");

            var dataView = _mlContext.Data.LoadFromEnumerable(historicalData);
            var trainTestSplit = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding("CountryEncoded", nameof(DemandForecastModelInput.Country))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("CityEncoded", nameof(DemandForecastModelInput.City)))
                .Append(_mlContext.Transforms.Concatenate("Features", "CountryEncoded", "CityEncoded", nameof(DemandForecastModelInput.Year), nameof(DemandForecastModelInput.Month), nameof(DemandForecastModelInput.CityAOV)))
                .Append(_mlContext.Regression.Trainers.FastTree(labelColumnName: "Label", featureColumnName: "Features"));

            var model = pipeline.Fit(trainTestSplit.TrainSet);

            // Başarı skorunu ölç (R2)
            var metrics = _mlContext.Regression.Evaluate(model.Transform(trainTestSplit.TestSet), labelColumnName: "Label");
            double actualAccuracy = Math.Max(0, metrics.RSquared);

            _mlContext.Model.Save(model, dataView.Schema, _modelPath);

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<DemandForecastModelInput, DemandForecastModelOutput>(model);

            var lastDate = allOrders.Max(x => x.CreatedDate);
            var targetMonth = lastDate.AddMonths(1).Month;
            var targetYear = lastDate.AddMonths(1).Year;

            // --- DARALTMA MANTIĞI (THRESHOLD) ---
            var filteredLocations = historicalData
                .GroupBy(x => new { x.Country, x.City })
                .Select(g => new {
                    loc = g.Key,
                    TotalHistoricalOrders = g.Sum(x => x.Label),
                    LastMonthOrders = g.Where(x => x.Year == lastDate.Year && x.Month == lastDate.Month).Sum(x => x.Label)
                })
                // KURAL: Toplamda en az 10 sipariş verilmiş OLMALI ve Son ayda en az 5 sipariş gelmiş OLMALI
                .Where(x => x.TotalHistoricalOrders >= 5 && x.LastMonthOrders >= 1)
                .Select(x => x.loc)
                .ToList();

            // Eski tahminleri temizleyelim (Yeni listeye göre güncellenecek)
            var oldForecasts = await _forecastRepo.GetAll().ToListAsync();
            _forecastRepo.RemoveRange(oldForecasts);

            foreach (var loc in filteredLocations)
            {
                var input = new DemandForecastModelInput
                {
                    Country = loc.Country,
                    City = loc.City,
                    Year = targetYear,
                    Month = targetMonth,
                    CityAOV = cityAovDict.ContainsKey(loc.City) ? cityAovDict[loc.City] : 100f
                };

                var prediction = predictionEngine.Predict(input);
                int predictedOrders = (int)Math.Max(0, Math.Round(prediction.Score));
                decimal predictedRevenue = (decimal)(predictedOrders * input.CityAOV);

                if (predictedOrders > 0) // Sadece 0'dan büyük tahminleri kaydedelim
                {
                    await _forecastRepo.AddAsync(new RegionalDemandForecast
                    {
                        Country = loc.Country,
                        City = loc.City,
                        TargetYear = targetYear,
                        TargetMonth = targetMonth,
                        PredictedRevenue = predictedRevenue,
                        PredictedOrderCount = predictedOrders,
                        ModelAccuracyScore = actualAccuracy,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    });
                }
            }

            await _uow.SaveAsync();
            return true;
        }
    }
}
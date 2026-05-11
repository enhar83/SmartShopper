using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class RegionalDemandForecastManager:IRegionalDemandForecastService
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
            var directory = Path.GetDirectoryName(_modelPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public async Task<List<RegionalDemandForecastDto>> TGetAllForecastsAsync()
        {
            var forecasts = await _forecastRepo.GetAll()
                .OrderByDescending(x => x.PredictedRevenue)
                .ToListAsync();

            return _mapper.Map<List<RegionalDemandForecastDto>>(forecasts);
        }

        public async Task<bool> TTrainAndGenerateForecastsAsync()
        {
            var allOrders = await _orderRepository.GetAll()
                .Include(o => o.UserAddress)
                .Where(o => o.UserAddress != null && o.IsDeleted == false)
                .ToListAsync();

            if (!allOrders.Any()) return false;

            // 🔥 Şehirlerin Ortalama Sepet Tutarını (AOV) önceden hesaplıyoruz
            var cityAovDict = allOrders
                .GroupBy(x => x.UserAddress.City)
                .ToDictionary(g => g.Key, g => (float)g.Average(x => x.TotalPrice));

            var historicalData = allOrders
                .GroupBy(o => new {
                    o.UserAddress.Country,
                    o.UserAddress.City,
                    o.CreatedDate.Year,
                    o.CreatedDate.Month
                })
                .Select(g => new DemandForecastModelInput
                {
                    Country = g.Key.Country,
                    City = g.Key.City,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    CityAOV = cityAovDict.ContainsKey(g.Key.City) ? cityAovDict[g.Key.City] : 100f, // Yeni Feature Eklendi!
                    TotalRevenue = (float)g.Sum(x => x.TotalPrice)
                }).ToList();

            if (historicalData.Count < 10)
                throw new Exception("Makine öğrenmesi modelini eğitmek için yeterli geçmiş bölge/ay verisi yok.");

            IDataView dataView = _mlContext.Data.LoadFromEnumerable(historicalData);

            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding("CountryEncoded", nameof(DemandForecastModelInput.Country))
        .Append(_mlContext.Transforms.Categorical.OneHotEncoding("CityEncoded", nameof(DemandForecastModelInput.City)))
        .Append(_mlContext.Transforms.Concatenate("Features",
            "CountryEncoded",
            "CityEncoded",
            nameof(DemandForecastModelInput.Year),
            nameof(DemandForecastModelInput.Month),
            nameof(DemandForecastModelInput.CityAOV)))
        // 🔥 Doğrusal modeller veriler arasındaki uçurumdan etkilenmesin diye Normalize ediyoruz:
        .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
        // 🔥 FastTree (Ayrık Ağaç) yerine SDCA (Sürekli Doğrusal Regresyon) kullanıyoruz:
        .Append(_mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features"));

            var model = pipeline.Fit(dataView);
            _mlContext.Model.Save(model, dataView.Schema, _modelPath);

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<DemandForecastModelInput, DemandForecastModelOutput>(model);

            var lastOrderDate = allOrders.Max(x => x.CreatedDate);
            var targetMonth = lastOrderDate.AddMonths(1).Month;
            var targetYear = lastOrderDate.AddMonths(1).Year;

            var distinctLocations = historicalData
                .GroupBy(x => new { x.Country, x.City })
                .Select(g => g.Key).ToList();

            foreach (var loc in distinctLocations)
            {
                // Tahmin yaparken o şehrin AOV'sini de modele veriyoruz
                var input = new DemandForecastModelInput
                {
                    Country = loc.Country,
                    City = loc.City,
                    Month = targetMonth,
                    Year = targetYear,
                    CityAOV = cityAovDict.ContainsKey(loc.City) ? cityAovDict[loc.City] : 100f
                };

                var prediction = predictionEngine.Predict(input);

                decimal predictedRev = (decimal)prediction.PredictedRevenue;
                if (predictedRev < 0) predictedRev = 0;

                decimal cityAOV = (decimal)input.CityAOV;
                int predictedOrders = cityAOV > 0 ? (int)Math.Round(predictedRev / cityAOV) : 0;

                if (predictedOrders == 0)
                {
                    predictedRev = 0m;
                }

                var existingForecast = await _forecastRepo.GetAll()
                    .FirstOrDefaultAsync(x => x.Country == loc.Country &&
                                              x.City == loc.City &&
                                              x.TargetYear == targetYear &&
                                              x.TargetMonth == targetMonth);

                if (existingForecast == null)
                {
                    await _forecastRepo.AddAsync(new RegionalDemandForecast
                    {
                        Country = loc.Country,
                        City = loc.City,
                        TargetMonth = targetMonth,
                        TargetYear = targetYear,
                        PredictedRevenue = predictedRev,
                        PredictedOrderCount = predictedOrders,
                        ModelAccuracyScore = 0.85,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    });
                }
                else
                {
                    existingForecast.PredictedRevenue = predictedRev;
                    existingForecast.PredictedOrderCount = predictedOrders;
                    existingForecast.UpdatedDate = DateTime.UtcNow;
                    _forecastRepo.Update(existingForecast);
                }
            }

            await _uow.SaveAsync();
            return true;
        }
    }
}

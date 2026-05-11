using Business_Layer.MLModels.SalesForecastingModels;
using Core_Layer.Dtos.SalesForecastDtos;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using System.Globalization;

namespace Business_Layer.Managers
{
    public class SalesForecastingManager : ISalesForecastingService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly string _modelPath;
        private readonly MLContext _mlContext;
        private readonly DateTime _referenceDate = new DateTime(2026, 5, 1); 

        public SalesForecastingManager(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
            _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "MLModels", "SalesForecastModel.zip");
            _mlContext = new MLContext(seed: 0);
        }

        public async Task<List<SalesForecastResultDto>> TGetSalesForecastAsync(int horizonMonths = 12)
        {
            var allOrders = await _orderRepository.GetAll().AsNoTracking().ToListAsync();

            var oneYearAgo = _referenceDate.AddMonths(-13); // son 13 ay esas alındı, 12 olmamasının nedeni window size değerini 6 yapabilmek için.

            var monthlyData = allOrders
                .Where(x => x.CreatedDate < _referenceDate && x.CreatedDate >= oneYearAgo)
                .GroupBy(x => new { x.CreatedDate.Year, x.CreatedDate.Month }) // yıla ve aya göre gruplandırma işlemi yapılıyor.
                .OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month) // ilk olarak yıla ardından ise aya göre bir sıralama işlemi gerçekleştiliyor.
                .Select(g => new SalesForecastingModelInput
                {
                    TotalSales = (float)g.Sum(s => (double)s.TotalPrice)
                })
                .ToList();

            if (monthlyData.Count < 5)
                throw new Exception("Insufficient up-to-date data was found. Verification of at least 5 months' worth of transactions from the last year is required.");

            IDataView dataView = _mlContext.Data.LoadFromEnumerable(monthlyData);

            //ssa kullanılmasının sebebi zaman serisi verilerini trend, mevsimsellik ve gürültü olarak parçalara ayırabilmesidir.
            var pipeline = _mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(SalesForecastingModelOutput.Score),
                inputColumnName: nameof(SalesForecastingModelInput.TotalSales),
                windowSize: 6, // 6 aylık periyotlara bakarak döngüleri yakalar.         
                seriesLength: monthlyData.Count,
                trainSize: monthlyData.Count,
                horizon: horizonMonths, //gelecekte kaç ay sonrasına bakılacağını belirler.
                confidenceLevel: 0.95f, //tahminler %95 güven aralığında hesaplanır.
                variableHorizon: true);

            var model = pipeline.Fit(dataView); //hazırlanan veri seti algoritmaya verilir.
            var forecastingEngine = model.CreateTimeSeriesEngine<SalesForecastingModelInput, SalesForecastingModelOutput>(_mlContext);
            var forecasts = forecastingEngine.Predict();

            var results = new List<SalesForecastResultDto>();

            for (int i = 0; i < monthlyData.Count; i++)
            {
                var date = _referenceDate.AddMonths(i - monthlyData.Count);
                results.Add(new SalesForecastResultDto
                {
                    Period = date.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    ActualAmount = Math.Round((double)monthlyData[i].TotalSales, 2),
                    IsForecast = false
                });
            }

            var lastActual = results.Last();
            results.Add(new SalesForecastResultDto
            {
                Period = lastActual.Period,
                ActualAmount = 0,
                ForecastAmount = lastActual.ActualAmount,
                IsForecast = true
            });

            for (int i = 0; i < horizonMonths; i++)
            {
                var forecastDate = _referenceDate.AddMonths(i);
                results.Add(new SalesForecastResultDto
                {
                    Period = forecastDate.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    ActualAmount = 0,
                    ForecastAmount = Math.Round((double)forecasts.Score[i], 2),
                    IsForecast = true
                });
            }

            return results;
        }

        public Task<bool> TTrainForecastModelAsync() => Task.FromResult(true);
    }
}
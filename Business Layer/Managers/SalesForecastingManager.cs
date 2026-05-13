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
            _mlContext = new MLContext(seed: 42); 
        }

        public async Task<List<SalesForecastResultDto>> TGetSalesForecastAsync(int horizonMonths = 6)
        {
            var allOrders = await _orderRepository.GetAll().AsNoTracking().ToListAsync();

            
            var historicalLimit = _referenceDate.AddMonths(-36);

            var monthlyData = allOrders
                .Where(x => x.CreatedDate < _referenceDate && x.CreatedDate >= historicalLimit)
                .GroupBy(x => new { x.CreatedDate.Year, x.CreatedDate.Month })
                .OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month)
                .Select(g => new SalesForecastingModelInput
                {
                    TotalSales = (float)g.Sum(s => (double)s.TotalPrice)
                })
                .ToList();

            
            if (monthlyData.Count < 25)
                throw new Exception("Yapay zekanın yıllık döngüleri (mevsimselliği) öğrenebilmesi için en az 25 aylık geçmiş satış verisine ihtiyacı vardır.");

            IDataView dataView = _mlContext.Data.LoadFromEnumerable(monthlyData);

            var pipeline = _mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(SalesForecastingModelOutput.Score),
                inputColumnName: nameof(SalesForecastingModelInput.TotalSales),
                windowSize: 12, 
                seriesLength: monthlyData.Count,
                trainSize: monthlyData.Count,
                horizon: horizonMonths,
                confidenceLevel: 0.95f,
                confidenceLowerBoundColumn: "LowerBoundSales",
                confidenceUpperBoundColumn: "UpperBoundSales",
                variableHorizon: true);

            var model = pipeline.Fit(dataView);
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
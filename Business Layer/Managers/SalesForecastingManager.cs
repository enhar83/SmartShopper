using Business_Layer.MLModels.SalesForecastingModels;
using Core_Layer.Dtos.SalesForecastDtos;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
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
            var orders = await _orderRepository.GetAll().AsNoTracking().ToListAsync();

            var monthlyData = orders
                .Where(x => x.CreatedDate < _referenceDate)
                .GroupBy(x => new { x.CreatedDate.Year, x.CreatedDate.Month })
                .OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month)
                .Select((g, index) => new SalesForecastingModelInput
                {
                    MonthIndex = (float)(index + 1),
                    TotalSales = (float)g.Sum(s => (double)s.TotalPrice)
                })
                .ToList();

            if (monthlyData.Count < 3)
                throw new Exception("Sales forecasting requires at least 3 months of historical data.");

            await TTrainForecastModelAsync();

            ITransformer trainedModel = _mlContext.Model.Load(_modelPath, out _);
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<SalesForecastingModelInput, SalesForecastingModelOutput>(trainedModel);

            var results = new List<SalesForecastResultDto>();

            foreach (var item in monthlyData)
            {
                int monthsDiff = (int)(item.MonthIndex - monthlyData.Count - 1);
                var dateOfIndex = _referenceDate.AddMonths(monthsDiff);

                results.Add(new SalesForecastResultDto
                {
                    Period = dateOfIndex.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    ActualAmount = Math.Round((double)item.TotalSales, 2),
                    ForecastAmount = 0,
                    IsForecast = false
                });
            }

            int lastIndex = monthlyData.Count;
            for (int i = 1; i <= horizonMonths; i++)
            {
                float nextIndex = (float)(lastIndex + i);
                var prediction = predictionEngine.Predict(new SalesForecastingModelInput { MonthIndex = nextIndex });

                var forecastDate = _referenceDate.AddMonths(i - 1); 

                results.Add(new SalesForecastResultDto
                {
                    Period = forecastDate.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    ActualAmount = 0,
                    ForecastAmount = Math.Round((double)prediction.Score, 2),
                    IsForecast = true
                });
            }

            return results;
        }

        public async Task<bool> TTrainForecastModelAsync()
        {
            try
            {
                var orders = await _orderRepository.GetAll().AsNoTracking().ToListAsync();

                var trainingData = orders
                    .Where(x => x.CreatedDate < _referenceDate)
                    .GroupBy(x => new { x.CreatedDate.Year, x.CreatedDate.Month })
                    .OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month)
                    .Select((g, index) => new SalesForecastingModelInput
                    {
                        MonthIndex = (float)(index + 1),
                        TotalSales = (float)g.Sum(s => (double)s.TotalPrice)
                    }).ToList();

                if (trainingData.Count < 3) return false;

                IDataView dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

                var pipeline = _mlContext.Transforms.CopyColumns("Label", nameof(SalesForecastingModelInput.TotalSales))
                    .Append(_mlContext.Transforms.Concatenate("Features", nameof(SalesForecastingModelInput.MonthIndex)))
                    .Append(_mlContext.Regression.Trainers.FastTree(labelColumnName: "Label", featureColumnName: "Features"));

                var model = pipeline.Fit(dataView);

                var directory = Path.GetDirectoryName(_modelPath);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory!);

                _mlContext.Model.Save(model, dataView.Schema, _modelPath);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
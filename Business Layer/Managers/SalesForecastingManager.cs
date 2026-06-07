using Business_Layer.MLModels.SalesForecastingModels;
using Core_Layer.Dtos.SalesForecastDtos;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Business_Layer.Managers
{
    public class SalesForecastingManager : ISalesForecastingService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly string _modelPath;
        private readonly string _metricPath;
        private readonly MLContext _mlContext;

        // Sistemin Analiz Baz Tarihi
        private readonly DateTime _referenceDate = new DateTime(2026, 5, 1);

        public SalesForecastingManager(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
            _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "MLModels", "SalesForecastModel.zip");
            _metricPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "MLModels", "SalesForecastMetrics.json");

            // Sonuçların deterministik (her çalışmada aynı ve tutarlı) olması için seed sabitlenir
            _mlContext = new MLContext(seed: 42);

            var dir = Path.GetDirectoryName(_modelPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);
        }

        public async Task<bool> TTrainForecastModelAsync()
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
                }).ToList();

            if (monthlyData.Count < 25)
                throw new Exception("Artificial intelligence needs at least 25 months of data to learn about seasonality.");

            double avgSales = monthlyData.Average(x => x.TotalSales);
            double stdDev = Math.Sqrt(monthlyData.Average(v => Math.Pow(v.TotalSales - avgSales, 2)));

            double upperBound = avgSales + (1.5 * stdDev);
            double lowerBound = Math.Max(0, avgSales - (1.5 * stdDev));

            foreach (var item in monthlyData)
            {
                if (item.TotalSales > upperBound) item.TotalSales = (float)upperBound;
                if (item.TotalSales < lowerBound) item.TotalSales = (float)lowerBound;
            }

            int validationMonths = 3;
            var trainDataList = monthlyData.Take(monthlyData.Count - validationMonths).ToList();
            var testDataList = monthlyData.Skip(monthlyData.Count - validationMonths).ToList();

            IDataView trainDataView = _mlContext.Data.LoadFromEnumerable(trainDataList);

            var evalPipeline = _mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(SalesForecastingModelOutput.Score),
                inputColumnName: nameof(SalesForecastingModelInput.TotalSales),
                windowSize: 6, 
                seriesLength: trainDataList.Count,
                trainSize: trainDataList.Count,
                horizon: validationMonths,
                confidenceLevel: 0.95f,
                confidenceLowerBoundColumn: nameof(SalesForecastingModelOutput.LowerBoundSales),
                confidenceUpperBoundColumn: nameof(SalesForecastingModelOutput.UpperBoundSales));

            var evalModel = evalPipeline.Fit(trainDataView);
            var evalEngine = evalModel.CreateTimeSeriesEngine<SalesForecastingModelInput, SalesForecastingModelOutput>(_mlContext);
            var validationForecast = evalEngine.Predict();

            double maeSum = 0, mseSum = 0, mapeSum = 0;
            int validMapeCount = 0;

            for (int i = 0; i < validationMonths; i++)
            {
                double actual = testDataList[i].TotalSales;
                double predicted = validationForecast.Score[i];
                double error = actual - predicted;

                maeSum += Math.Abs(error);
                mseSum += (error * error);

                if (actual > 0)
                {
                    mapeSum += Math.Abs(error / actual);
                    validMapeCount++;
                }
            }

            var report = new ForecastEvaluationReportDto
            {
                MeanAbsoluteError = Math.Round(maeSum / validationMonths, 2),
                RootMeanSquaredError = Math.Round(Math.Sqrt(mseSum / validationMonths), 2),
                MeanAbsolutePercentageError = validMapeCount > 0 ? Math.Round((mapeSum / validMapeCount) * 100, 2) : 0,
                EvaluatedAt = DateTime.Now
            };

            await File.WriteAllTextAsync(_metricPath, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));

            IDataView fullDataView = _mlContext.Data.LoadFromEnumerable(monthlyData);

            var finalPipeline = _mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(SalesForecastingModelOutput.Score),
                inputColumnName: nameof(SalesForecastingModelInput.TotalSales),
                windowSize: 6, 
                seriesLength: monthlyData.Count,
                trainSize: monthlyData.Count,
                horizon: 12,
                confidenceLevel: 0.95f,
                confidenceLowerBoundColumn: nameof(SalesForecastingModelOutput.LowerBoundSales),
                confidenceUpperBoundColumn: nameof(SalesForecastingModelOutput.UpperBoundSales));

            var finalModel = finalPipeline.Fit(fullDataView);
            _mlContext.Model.Save(finalModel, fullDataView.Schema, _modelPath);

            return true;
        }

        public async Task<List<SalesForecastResultDto>> TGetSalesForecastAsync(int horizonMonths = 6)
        {
            if (!File.Exists(_modelPath))
                throw new Exception("Before a forecast report can be generated, the artificial intelligence model must first be trained.");

            var allOrders = await _orderRepository.GetAll().AsNoTracking().ToListAsync();
            var historicalLimit = _referenceDate.AddMonths(-36);

            var monthlyData = allOrders
                .Where(x => x.CreatedDate < _referenceDate && x.CreatedDate >= historicalLimit)
                .GroupBy(x => new { x.CreatedDate.Year, x.CreatedDate.Month })
                .OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month)
                .Select(g => new SalesForecastingModelInput { TotalSales = (float)g.Sum(s => (double)s.TotalPrice) })
                .ToList();

            ITransformer trainedModel;
            using (var stream = new FileStream(_modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = _mlContext.Model.Load(stream, out _);
            }

            var forecastingEngine = trainedModel.CreateTimeSeriesEngine<SalesForecastingModelInput, SalesForecastingModelOutput>(_mlContext);
            var forecasts = forecastingEngine.Predict();

            var results = new List<SalesForecastResultDto>();

            for (int i = 0; i < monthlyData.Count; i++)
            {
                results.Add(new SalesForecastResultDto
                {
                    Period = _referenceDate.AddMonths(i - monthlyData.Count).ToString("MMM yyyy", CultureInfo.InvariantCulture),
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
                LowerBound = lastActual.ActualAmount,
                UpperBound = lastActual.ActualAmount,
                IsForecast = true
            });

            for (int i = 0; i < horizonMonths; i++)
            {
                double forecastVal = Math.Max(0, forecasts.Score[i]);
                double lowerVal = Math.Max(0, forecasts.LowerBoundSales[i]);
                double upperVal = Math.Max(0, forecasts.UpperBoundSales[i]);

                results.Add(new SalesForecastResultDto
                {
                    Period = _referenceDate.AddMonths(i).ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    ActualAmount = 0,
                    ForecastAmount = Math.Round(forecastVal, 2),
                    LowerBound = Math.Round(lowerVal, 2),
                    UpperBound = Math.Round(upperVal, 2),
                    IsForecast = true
                });
            }

            return results;
        }

        public async Task<ForecastEvaluationReportDto> TGetForecastMetricsAsync()
        {
            if (!File.Exists(_metricPath))
                return new ForecastEvaluationReportDto { MeanAbsoluteError = 0, RootMeanSquaredError = 0, MeanAbsolutePercentageError = 0, EvaluatedAt = _referenceDate };

            var json = await File.ReadAllTextAsync(_metricPath);
            return JsonSerializer.Deserialize<ForecastEvaluationReportDto>(json) ?? new ForecastEvaluationReportDto();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Business_Layer.MLModels.AnomalyModels;
using Core_Layer.Dtos.OrderAnomalyDtos;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

namespace Business_Layer.Managers
{
    public class OrderAnomalyManager : IOrderAnomalyService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderAnomalyResultRepository _anomalyRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly MLContext _mlContext;
        private readonly string _metricPath;

        // Proje standart referans tarihi
        private readonly DateTime _referenceDate = new DateTime(2026, 5, 11, 0, 0, 0, DateTimeKind.Utc);

        public OrderAnomalyManager(
            IOrderRepository orderRepository,
            IOrderAnomalyResultRepository anomalyRepo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _anomalyRepo = anomalyRepo;
            _uow = uow;
            _mapper = mapper;

            _mlContext = new MLContext(seed: 42);
            _metricPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "MLModels", "AnomalyMetrics.json");
        }

        public async Task<List<OrderAnomalyDto>> TGetAllAnomaliesAsync()
        {
            var anomalies = await _anomalyRepo.GetAll()
                .Include(x => x.Order)
                .ThenInclude(o => o.AppUser)
                .AsNoTracking()
                .OrderBy(x => x.PValue)
                .ToListAsync();

            return _mapper.Map<List<OrderAnomalyDto>>(anomalies);
        }

        public async Task<List<CustomerOrderHistoryDto>> TGetCustomerOrderHistoryAsync(Guid orderId)
        {
            var targetOrder = await _orderRepository.GetAll().FirstOrDefaultAsync(x => x.Id == orderId);
            if (targetOrder == null) return new List<CustomerOrderHistoryDto>();

            var history = await _orderRepository.GetAll()
                .Where(x => x.AppUserId == targetOrder.AppUserId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return _mapper.Map<List<CustomerOrderHistoryDto>>(history);
        }

        public async Task<bool> TRunAnomalyDetectionAsync()
        {
            var cutoffDate = _referenceDate.AddDays(-180);

            var orders = await _orderRepository.GetAll()
                .Where(x => !x.IsDeleted && x.CreatedDate >= cutoffDate)
                .OrderBy(x => x.CreatedDate)
                .ToListAsync();

            if (orders.Count < 30)
                throw new Exception("Anomaly detection is not possible without at least 30 order records (Time Series).");

            var inputData = orders.Select(x => new AnomalyModelInput
            {
                TotalPrice = (float)x.TotalPrice
            }).ToList();

            IDataView dataView = _mlContext.Data.LoadFromEnumerable(inputData);

            int trainingSize = Math.Min(orders.Count, 90);
            int seasonalitySize = 7; 

            double confidenceLevel = 98.0; 

            var pipeline = _mlContext.Transforms.DetectSpikeBySsa(
                outputColumnName: nameof(AnomalyModelOutput.Prediction),
                inputColumnName: nameof(AnomalyModelInput.TotalPrice),
                confidence: confidenceLevel,
                pvalueHistoryLength: Math.Max(10, trainingSize / 3),
                trainingWindowSize: trainingSize,
                seasonalityWindowSize: seasonalitySize
            );

            var model = pipeline.Fit(dataView);
            var transformedData = model.Transform(dataView);
            var predictions = _mlContext.Data.CreateEnumerable<AnomalyModelOutput>(transformedData, reuseRowObject: false).ToList();

            bool isAnomalyFound = false;
            int totalAnomalies = 0;

            for (int i = 0; i < predictions.Count; i++)
            {
                var result = predictions[i];
                var targetOrder = orders[i];

                if (result.Prediction[0] == 1)
                {
                    totalAnomalies++;
                    var existing = await _anomalyRepo.GetAll().FirstOrDefaultAsync(x => x.OrderId == targetOrder.Id);

                    if (existing == null)
                    {
                        await _anomalyRepo.AddAsync(new OrderAnomalyResult
                        {
                            OrderId = targetOrder.Id,
                            Score = result.Prediction[1],
                            PValue = result.Prediction[2],
                            IsAnomaly = true,
                            Description = $"System Anomaly: The transaction of {targetOrder.TotalPrice} TL is outside the {confidenceLevel}% statistical confidence interval.",
                            CreatedDate = _referenceDate,
                            IsDeleted = false
                        });

                        isAnomalyFound = true;
                    }
                }
            }

            if (isAnomalyFound)
                await _uow.SaveAsync();

            var report = new AnomalyEvaluationReportDto
            {
                ConfidenceLevel = confidenceLevel,
                TotalAnalyzedOrders = orders.Count,
                TotalAnomaliesFound = totalAnomalies,
                AnomalyDetectionRate = Math.Round(((double)totalAnomalies / orders.Count) * 100, 2),
                EvaluatedAt = _referenceDate
            };

            var directory = Path.GetDirectoryName(_metricPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            await File.WriteAllTextAsync(_metricPath, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));

            return true;
        }

        public async Task<AnomalyEvaluationReportDto> TGetAnomalyMetricsAsync()
        {
            if (!File.Exists(_metricPath))
            {
                return new AnomalyEvaluationReportDto { ConfidenceLevel = 0, TotalAnalyzedOrders = 0, TotalAnomaliesFound = 0, AnomalyDetectionRate = 0, EvaluatedAt = _referenceDate };
            }

            var jsonContent = await File.ReadAllTextAsync(_metricPath);
            return JsonSerializer.Deserialize<AnomalyEvaluationReportDto>(jsonContent) ?? new AnomalyEvaluationReportDto();
        }
    }
}
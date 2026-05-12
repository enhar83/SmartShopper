using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Business_Layer.MLModels.AnomalyModels;
using Core_Layer.Dtos.OrderAnomalyDtos;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Entity_Layer.Common;
using MailKit.Search;
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
        }

        public async Task<List<OrderAnomalyDto>> TGetAllAnomaliesAsync()
        {
            var anomalies = await _anomalyRepo.GetAll()
                .Include(x => x.Order)
                .ThenInclude(o => o.AppUser)
                .OrderBy(x => x.PValue)
                .ToListAsync();

            return _mapper.Map<List<OrderAnomalyDto>>(anomalies);
        }

        public async Task<List<CustomerOrderHistoryDto>> TGetCustomerOrderHistoryAsync(Guid orderId )
        {
            var targetOrder = await _orderRepository.GetAll()
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (targetOrder == null) return new List<CustomerOrderHistoryDto>();

            var history = await _orderRepository.GetAll()
                .Where(x => x.AppUserId == targetOrder.AppUserId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return _mapper.Map<List<CustomerOrderHistoryDto>>(history);
        }

        public async Task<bool> TRunAnomalyDetectionAsync()
        {
            var orders = await _orderRepository.GetAll()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.CreatedDate)
                .ToListAsync();

            if (orders.Count < 12) 
                throw new Exception("At least 12 order records are required for anomaly detection.");

            var inputData = orders.Select(x => new AnomalyModelInput
            {
                TotalPrice = (float)x.TotalPrice
            }).ToList();

            IDataView dataView = _mlContext.Data.LoadFromEnumerable(inputData);

            var pipeline = _mlContext.Transforms.DetectSpikeBySsa(
                outputColumnName: nameof(AnomalyModelOutput.Prediction),
                inputColumnName: nameof(AnomalyModelInput.TotalPrice),
                confidence: 99.0, 
                pvalueHistoryLength: orders.Count / 4, 
                trainingWindowSize: orders.Count, 
                seasonalityWindowSize: 3 
            );

            var model = pipeline.Fit(dataView);
            var transformedData = model.Transform(dataView);

            var predictions = _mlContext.Data.CreateEnumerable<AnomalyModelOutput>(transformedData, reuseRowObject: false).ToList();

            for (int i = 0; i < predictions.Count; i++)
            {
                var result = predictions[i];

                if (result.Prediction[0] == 1)
                {
                    var targetOrder = orders[i];

                    var existing = await _anomalyRepo.GetAll()
                        .FirstOrDefaultAsync(x => x.OrderId == targetOrder.Id);

                    if (existing == null)
                    {
                        await _anomalyRepo.AddAsync(new OrderAnomalyResult
                        {
                            OrderId = targetOrder.Id,
                            Score = result.Prediction[1],
                            PValue = result.Prediction[2], 
                            IsAnomaly = true,
                            Description = $"Suspicious transaction detection: The order amount (${targetOrder.TotalPrice}) shows a jump far outside the general selling trend.",
                            CreatedDate = DateTime.UtcNow,
                            IsDeleted = false
                        });
                    }
                }
            }

            await _uow.SaveAsync();
            return true;
        }
    }
}


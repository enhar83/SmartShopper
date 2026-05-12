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
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

namespace Business_Layer.Managers
{
    public class OrderAnomalyManager:IOrderAnomalyService
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
            // Sadece anomalileri, en riskli olandan başlayarak çekiyoruz (P-Value ne kadar küçükse risk o kadar büyüktür)
            var anomalies = await _anomalyRepo.GetAll()
                .Include(x => x.Order)
                .ThenInclude(o => o.AppUser)
                .OrderBy(x => x.PValue)
                .ToListAsync();

            return _mapper.Map<List<OrderAnomalyDto>>(anomalies);
        }

        public async Task<bool> TRunAnomalyDetectionAsync()
        {
            // 1. ADIM: Tüm siparişleri kronolojik olarak çek
            var orders = await _orderRepository.GetAll()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.CreatedDate)
                .ToListAsync();

            if (orders.Count < 12) // Algoritmanın sağlıklı çalışması için minimum veri seti
                throw new Exception("Anomali tespiti için en az 12 sipariş kaydı gerekmektedir.");

            // 2. ADIM: Veriyi ML.NET formatına çevir
            var inputData = orders.Select(x => new AnomalyModelInput
            {
                TotalPrice = (float)x.TotalPrice
            }).ToList();

            IDataView dataView = _mlContext.Data.LoadFromEnumerable(inputData);

            // 3. ADIM: SSA (Singular Spectrum Analysis) Spike Detection Pipeline
            // Confidence: %99 (Sadece çok bariz sapmaları yakala)
            // PvalueHistoryLength: Analiz penceresi (Verinin %25'i kadar geçmişe bakar)
            // Metodun içindeki pipeline kısmını bununla değiştir:
            var pipeline = _mlContext.Transforms.DetectSpikeBySsa(
                outputColumnName: nameof(AnomalyModelOutput.Prediction),
                inputColumnName: nameof(AnomalyModelInput.TotalPrice),
                confidence: 99.0, // Güven aralığı (%)
                pvalueHistoryLength: orders.Count / 4, // Kayar pencere boyutu
                trainingWindowSize: orders.Count, // Toplam eğitim verisi boyutu
                seasonalityWindowSize: 3 // Mevsimsellik penceresi (küçük bir değer tutarlılık sağlar)
            );

            var model = pipeline.Fit(dataView);
            var transformedData = model.Transform(dataView);

            // 4. ADIM: Tahminleri oku ve sadece "Anomali" olanları ayıkla
            var predictions = _mlContext.Data.CreateEnumerable<AnomalyModelOutput>(transformedData, reuseRowObject: false).ToList();

            for (int i = 0; i < predictions.Count; i++)
            {
                var result = predictions[i];

                // Prediction[0] == 1 ise bu bir anomalidir (Spike)
                if (result.Prediction[0] == 1)
                {
                    var targetOrder = orders[i];

                    // Zaten kaydedilmiş mi kontrol et (One-to-One ilişki gereği)
                    var existing = await _anomalyRepo.GetAll()
                        .FirstOrDefaultAsync(x => x.OrderId == targetOrder.Id);

                    if (existing == null)
                    {
                        await _anomalyRepo.AddAsync(new OrderAnomalyResult
                        {
                            OrderId = targetOrder.Id,
                            Score = result.Prediction[1], // O anki fiyat
                            PValue = result.Prediction[2], // İstatistikel olasılık
                            IsAnomaly = true,
                            Description = $"Şüpheli işlem tespiti: Sipariş tutarı ($ {targetOrder.TotalPrice}) genel satış trendinin çok dışında bir sıçrama gösteriyor.",
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


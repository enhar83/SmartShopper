using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics.Tensors;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Business_Layer.MLModels.CustomerSegmentationModels;
using Core_Layer.Dtos.CustomerSegmentationDtos;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Business_Layer.Managers
{
    public class CustomerSegmentationManager:ICustomerSegmentationService
    {
        private readonly UserManager<AppUser> _userManager; //ml eğitimi için gereken ham veriyi dbden çekmek için kullanılır.
        private readonly ICustomerSegmentationResultRepository _customerSegmentationResultRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerSegmentationManager> _logger;
        private readonly string _modelPath;
        private readonly MLContext _mlContext; //ml.net için dbcontexttir, veri yükleme, pipeline oluşturma, model eğitme ve kaydetme gibi tüm işlemler bu nesne üzerinden yürütülür. seed:1 parametresi ile eğitimin her zaman aynı sonuçları vermesi sağlanır.

        public CustomerSegmentationManager(UserManager<AppUser> userManager, ICustomerSegmentationResultRepository customerSegmentationResultRepository, IUnitOfWork uow, IMapper mapper, ILogger<CustomerSegmentationManager> logger)
        {
            _userManager = userManager;
            _customerSegmentationResultRepository = customerSegmentationResultRepository;
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
            _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "MLModels", "CustomerSegmentationModel.zip");
            _mlContext = new MLContext(seed: 0);
        }

        public async Task<List<CustomerSegmentResultDto>> TGetSegmentationResultsAsync()
        {
            _logger.LogInformation("Dashboard için segmentasyon sonuçları çekiliyor.");

            var results = await _customerSegmentationResultRepository.GetAll()
                .Include(x => x.AppUser)
                .AsNoTracking()
                .OrderByDescending(x => x.LastUpdated)
                .ToListAsync();

            return _mapper.Map<List<CustomerSegmentResultDto>>(results);
        }

        public async Task<CustomerSegmentDto> TGetUserSegmentAsync(Guid userId)
        {
            var user = await _userManager.Users
        .Include(u => u.Orders)
        .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            ITransformer trainedModel = _mlContext.Model.Load(_modelPath, out _);
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<CustomerSegmentationModelInput, CustomerSegmentationModelOutput>(trainedModel);

            var userOrders = user.Orders ?? new List<Order>();

            var input = new CustomerSegmentationModelInput
            {
                TotalSpend = (float)userOrders.Sum(o => o.TotalPrice),
                OrderCount = (float)userOrders.Count,
                DaysSinceLastOrder = userOrders.Any()
                    ? (float)(DateTime.Now - userOrders.Max(o => o.CreatedDate)).TotalDays
                    : 365f
            };

            var prediction = predictionEngine.Predict(input);

            return new CustomerSegmentDto
            {
                AppUserId = user.Id,
                UserFullName = $"{user.Name} {user.Surname}",
                SegmentLabel = prediction.Prediction ?? "Belirsiz",
                ConfidenceScore = prediction.Score?.Max() ?? 0,
                LastUpdated = DateTime.Now
            };
        }

        public async Task TProcessBatchSegmentationAsync()
        {
            try
            {
                _logger.LogInformation("Toplu segmentasyon işlemi (Batch Processing) başlatıldı.");

                if (!File.Exists(_modelPath))
                {
                    _logger.LogError("Eğitilmiş model dosyası bulunamadı! Yol: {Path}", _modelPath);
                    return;
                }

                
                ITransformer trainedModel = _mlContext.Model.Load(_modelPath, out var modelInputSchema); 
                var predictionEngine = _mlContext.Model.CreatePredictionEngine<CustomerSegmentationModelInput, CustomerSegmentationModelOutput>(trainedModel); //önceden eğitilen .zip dosyası belleğe yüklenir. PredoctionEngine girdi verilerini alıp eğitilmiş algoritmaya sokan ve çıktı üreten karar mekanizmasıdır.

                var users = await _userManager.Users
                    .Include(u => u.Orders)
                    .Include(u => u.SegmentationResults) 
                    .ToListAsync();

                _logger.LogInformation("{Count} adet kullanıcı işleniyor...", users.Count);

                //her kullanıcı tek tek ele alınır. Kullanıcının ham sipariş verileri ML modelinin anlayacağı sayısal ModelInput formatına çevrilir.
                foreach (var user in users)
                {
                    var userOrders = user.Orders ?? new List<Order>();

                    var input = new CustomerSegmentationModelInput
                    {
                        TotalSpend = (float)userOrders.Sum(o => o.TotalPrice),
                        OrderCount = (float)userOrders.Count,
                        DaysSinceLastOrder = userOrders.Any()
                            ? (float)(DateTime.Now - userOrders.Max(o => o.CreatedDate)).TotalDays
                            : 365f
                    };

                    var prediction = predictionEngine.Predict(input); //hazırlanan veri modele verilir ve model kullanıcının kümesini döner.

                    var results = user.SegmentationResults ?? new List<CustomerSegmentationResult>();
                    var existingResult = results.OrderByDescending(x => x.LastUpdated).FirstOrDefault();

                    if (existingResult != null) //dbde her seferinde yeni bir satır oluşturmak yerine, kullanıcının son segment kaydı bulunup güncellenir. Bu dbnin şişmesini engeller ve index performansını korur.
                    {
                        existingResult.SegmentLabel = prediction.Prediction ?? "Unknown";
                        existingResult.ConfidenceScore = prediction.Score != null ? prediction.Score.Max() : 0.0;
                        existingResult.LastUpdated = DateTime.Now;
                    }
                    else
                    {
                        user.SegmentationResults?.Add(new CustomerSegmentationResult
                        {
                            AppUserId = user.Id,
                            SegmentLabel = prediction.Prediction ?? "Unknown",
                            ConfidenceScore = prediction.Score != null ? prediction.Score.Max() : 0.0,
                            LastUpdated = DateTime.Now
                        });
                    }
                }

                await _uow.SaveAsync(); //uow ile her şey tek bir transaction içerisinde toplandı

                _logger.LogInformation("Toplu segmentasyon başarıyla tamamlandı.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Batch processing sırasında beklenmedik hata oluştu.");
            }
        }

        public async Task<bool> TTrainModelAsync()
        {
            try
            {
                _logger.LogInformation("ML Model Eğitim Süreci Başlatıldı.");

                var trainingData = await _userManager.Users
                    .Select(u => new CustomerSegmentationModelInput
                    {
                        TotalSpend = (float)(u.Orders.Select(o => o.TotalPrice).DefaultIfEmpty(0).Sum()),
                        OrderCount = (float)u.Orders.Count(),
                        DaysSinceLastOrder = u.Orders.Any()
                            ? (float)(DateTime.Now - u.Orders.Max(o => o.CreatedDate)).TotalDays
                            : 365f
                    })
                    .ToListAsync();

                if (trainingData.Count < 5) 
                {
                    _logger.LogWarning("Eğitim için yeterli veri yok (Kullanıcı Sayısı: {Count}).", trainingData.Count);
                    return false;
                }

                IDataView trainingDataView = _mlContext.Data.LoadFromEnumerable(trainingData);

                var pipeline = _mlContext.Transforms.Concatenate("Features", //concatenate ile rfm'deki 3 parametreyi birleştirip tek bir paket haline getirme işlemi yapılır. Model girdileri tek bir vektör (features) olarak bekler.
                        nameof(CustomerSegmentationModelInput.TotalSpend),
                        nameof(CustomerSegmentationModelInput.OrderCount),
                        nameof(CustomerSegmentationModelInput.DaysSinceLastOrder))
                    .Append(_mlContext.Transforms.NormalizeMinMax("Features")) //totalspend 10000 iken ordercount 5 olabilir. bu aradaki farkın sonuçları etkilememesi için normalizasyon yapılır. tüm değerler 0,1 aralığına çekilir.
                    .Append(_mlContext.Clustering.Trainers.KMeans(featureColumnName: "Features", numberOfClusters: 3)); //bu bir kümeleme algoritmasıdır. numberOfClusters:3 ile müşteri verilerine bakılır ve onları birbirine benzeyen 3 ana gruba ayrılır.

                _logger.LogInformation("Model eğitiliyor...");
                var trainedModel = pipeline.Fit(trainingDataView); //burada algoritma veriyi öğrenir. Matematiksel olarak küme merkezleri burada belirlenir.

                var directory = Path.GetDirectoryName(_modelPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _mlContext.Model.Save(trainedModel, trainingDataView.Schema, _modelPath); //persistance sağlanır. eğitilen model uçucu bir nesne olduğundan dolayı uygulama kapandığında sonuçların gitmtmesi için Save metodu ile disle bir .zip dosyası olarak yazılır. 

                _logger.LogInformation("Model başarıyla eğitildi: {Path}", _modelPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TrainModelAsync içerisinde kritik hata!");
                return false;
            }
        }
    }
}

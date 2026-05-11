using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Business_Layer.MLModels.ChurnPredictionModels;
using Core_Layer.Dtos.ChurnPredictionDtos;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Business_Layer.Managers 
{
    public class CustomerChurnResultManager : ICustomerChurnResultService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICustomerChurnResultRepository _churnRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly MLContext _mlContext;
        private readonly string _modelPath;

        private readonly DateTime _referenceDate = new DateTime(2026, 5, 11);

        public CustomerChurnResultManager(
            IOrderRepository orderRepository,
            UserManager<AppUser> userManager,
            ICustomerChurnResultRepository churnRepo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _userManager = userManager;
            _churnRepo = churnRepo;
            _uow = uow;
            _mapper = mapper;

            _mlContext = new MLContext(seed: 42);

            _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "MLModels", "ChurnModel.zip");
            var directory = Path.GetDirectoryName(_modelPath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public async Task<List<ChurnPredictionResultDto>> TGetAllChurnResultsAsync()
        {
            var churnResults = await _churnRepo.GetAll()
                .Include(x => x.AppUser)
                .OrderByDescending(x => x.ChurnProbability) 
                .ToListAsync();

            var resultList = new List<ChurnPredictionResultDto>();

            foreach (var item in churnResults)
            {
                var dto = _mapper.Map<ChurnPredictionResultDto>(item);
                dto.UserFullName = item.AppUser != null ? $"{item.AppUser.Name} {item.AppUser.Surname}" : "Bilinmeyen Kullanıcı";
                resultList.Add(dto);
            }

            return resultList;
        }

        private ChurnPredictionModelInput GetInputData(AppUser user, List<Order> userOrders)
        {
            var lastOrder = userOrders.OrderByDescending(x => x.CreatedDate).FirstOrDefault();

            float recency = lastOrder != null ? (float)(_referenceDate - lastOrder.CreatedDate).TotalDays : 365f;
            float frequency = userOrders.Count;
            float monetary = (float)userOrders.Sum(x => (double)x.TotalPrice);
            float averageOrderValue = frequency > 0 ? monetary / frequency : 0f;
            bool label = recency > 120;

            return new ChurnPredictionModelInput
            {
                TotalSpend = monetary,
                OrderCount = frequency,
                DaysSinceLastOrder = recency,
                AverageOrderValue = averageOrderValue,
                Label = label
            };
        }

        public async Task<bool> TTrainChurnModelAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var allOrders = await _orderRepository.GetAll().ToListAsync();

            var userOrdersDict = allOrders.GroupBy(x => x.AppUserId).ToDictionary(g => g.Key, g => g.ToList());

            var trainingData = users.Select(user => GetInputData(user, userOrdersDict.ContainsKey(user.Id) ? userOrdersDict[user.Id] : new List<Order>())).ToList();

            if (trainingData.Count < 5) return false;

            int churnedCount = trainingData.Count(x => x.Label == true);
            if (churnedCount == 0 || churnedCount == trainingData.Count)
            {
                throw new Exception("Modelin öğrenebilmesi için veritabanında hem inaktif (120 günden eski) hem de aktif müşteriler birlikte bulunmalıdır.");
            }

            IDataView dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            var pipeline = _mlContext.Transforms.Concatenate("Features",
                    nameof(ChurnPredictionModelInput.TotalSpend),
                    nameof(ChurnPredictionModelInput.OrderCount),
                    nameof(ChurnPredictionModelInput.AverageOrderValue))
                .Append(_mlContext.Transforms.NormalizeMinMax("Features")) 
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features"));

            var model = pipeline.Fit(dataView);

            _mlContext.Model.Save(model, dataView.Schema, _modelPath);
            return true;
        }

        public async Task<List<ChurnPredictionResultDto>> TProcessAllCustomersChurnAsync()
        {
            bool isTrained = await TTrainChurnModelAsync();
            if (!isTrained) throw new Exception("Yeterli veri olmadığı için analiz gerçekleştirilemedi.");

            ITransformer trainedModel;
            using (var stream = new FileStream(_modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = _mlContext.Model.Load(stream, out _);
            }

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ChurnPredictionModelInput, ChurnPredictionModelOutput>(trainedModel);

            var usersWithOrders = await _userManager.Users.Include(x => x.CustomerChurnResult).ToListAsync();
            var allOrders = await _orderRepository.GetAll().ToListAsync();
            var userOrdersDict = allOrders.GroupBy(x => x.AppUserId).ToDictionary(g => g.Key, g => g.ToList());

            var resultList = new List<ChurnPredictionResultDto>();

            foreach (var user in usersWithOrders)
            {
                var userOrders = userOrdersDict.ContainsKey(user.Id) ? userOrdersDict[user.Id] : new List<Order>();
                var input = GetInputData(user, userOrders);

                var churnEntity = user.CustomerChurnResult ?? new CustomerChurnResult
                {
                    AppUserId = user.Id,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                };

                var prediction = predictionEngine.Predict(input);

                decimal timeRisk = Math.Min((decimal)(input.DaysSinceLastOrder / 180f) * 100m, 100m);

                decimal behaviorRisk = (decimal)(prediction.Probability * 100);

                decimal finalRisk = (timeRisk * 0.6m) + (behaviorRisk * 0.4m);

                finalRisk = Math.Clamp(finalRisk, 1.5m, 99.0m);

                churnEntity.IsChurn = finalRisk > 50m;
                churnEntity.ChurnProbability = finalRisk;

                churnEntity.Recency = input.DaysSinceLastOrder;
                churnEntity.Frequency = input.OrderCount;
                churnEntity.Monetary = (decimal)input.TotalSpend;
                churnEntity.LastUpdated = DateTime.Now;

                if (user.CustomerChurnResult == null)
                    await _churnRepo.AddAsync(churnEntity);
                else
                {
                    churnEntity.UpdatedDate = DateTime.UtcNow;
                    _churnRepo.Update(churnEntity);
                }

                var dto = _mapper.Map<ChurnPredictionResultDto>(churnEntity);
                dto.UserFullName = $"{user.Name} {user.Surname}";

                resultList.Add(dto);
            }

            await _uow.SaveAsync();
            return resultList;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            _mlContext = new MLContext(seed: 1);
            _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "MLModels", "ChurnModel.zip");
        }

        private ChurnPredictionModelInput GetInputData(AppUser user)
        {
            var orders = user.Orders ?? new List<Order>();

            var lastOrder = orders.OrderByDescending(o => o.CreatedDate).FirstOrDefault();

            float recency = lastOrder != null ? (float)(_referenceDate - lastOrder.CreatedDate).TotalDays : 365f;
            float frequency = orders.Count;
            float monetary = (float)orders.Sum(o => (double)o.TotalPrice);

            return new ChurnPredictionModelInput
            {
                TotalSpend = monetary,
                OrderCount = frequency,
                DaysSinceLastOrder = recency,
                AverageOrderValue = frequency > 0 ? monetary / frequency : 0f,
                Label = recency > 180
            };
        }

        public async Task<List<ChurnPredictionResultDto>> TProcessAllCustomersChurnAsync()
        {
            await TTrainChurnModelAsync();
            ITransformer trainedModel = _mlContext.Model.Load(_modelPath, out _);
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ChurnPredictionModelInput, ChurnPredictionModelOutput>(trainedModel);

            var users = await _userManager.Users
                .Include(x => x.Orders)
                .Include(x => x.CustomerChurnResult)
                .ToListAsync();

            var results = new List<ChurnPredictionResultDto>();

            foreach (var user in users)
            {
                var input = GetInputData(user);

                var prediction = predictionEngine.Predict(input);

                var churnEntity = user.CustomerChurnResult;

                if (churnEntity == null)
                {
                    churnEntity = new CustomerChurnResult { AppUserId = user.Id };
                    await _churnRepo.AddAsync(churnEntity);
                }

                _mapper.Map(prediction, churnEntity);

                churnEntity.Recency = input.DaysSinceLastOrder;
                churnEntity.Frequency = input.OrderCount;
                churnEntity.Monetary = (decimal)input.TotalSpend;
                churnEntity.LastUpdated = DateTime.Now;

                _churnRepo.Update(churnEntity);

                var dto = _mapper.Map<ChurnPredictionResultDto>(churnEntity);
                dto.UserFullName = $"{user.Name} {user.Surname}";
                results.Add(dto);
            }

            await _uow.SaveAsync();

            return results;
        }

        public async Task<bool> TTrainChurnModelAsync()
        {
            var users = await _userManager.Users.Include(x => x.Orders).ToListAsync();
            var trainingData = users.Select(u => GetInputData(u)).ToList();

            if (trainingData.Count < 5) return false;

            IDataView dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            var pipeline = _mlContext.Transforms.Concatenate("Features",
                nameof(ChurnPredictionModelInput.TotalSpend),
                nameof(ChurnPredictionModelInput.OrderCount),
                nameof(ChurnPredictionModelInput.DaysSinceLastOrder),
                nameof(ChurnPredictionModelInput.AverageOrderValue))
                .Append(_mlContext.BinaryClassification.Trainers.FastTree());

            var model = pipeline.Fit(dataView);
            _mlContext.Model.Save(model, dataView.Schema, _modelPath);

            return true;
        }
    }
}

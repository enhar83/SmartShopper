using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Business_Layer.MLModels.ProductForecastModels;
using Core_Layer.Dtos.ProductForecastDtos;
using Core_Layer.IRepositories;
using Core_Layer.IServices;
using Entity_Layer;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

namespace Business_Layer.Managers
{
    public class ProductSalesForecastManager : IProductSalesForecastService
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductSalesForecastRepository _forecastRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly MLContext _mlContext;
        private readonly string _modelPath;

        public ProductSalesForecastManager(
            IOrderItemRepository orderItemRepository,
            IProductRepository productRepository,
            IProductSalesForecastRepository forecastRepo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _forecastRepo = forecastRepo;
            _uow = uow;
            _mapper = mapper;

            _mlContext = new MLContext(seed: 42);
            _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "MLModels", "ProductForecastModel.zip");

            var directory = Path.GetDirectoryName(_modelPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public async Task<List<ProductSalesForecastDto>> TGetAllForecastsAsync()
        {
            var forecasts = await _forecastRepo.GetAll()
                .Include(x => x.Product)
                .OrderByDescending(x => x.ExpectedRevenue)
                .ToListAsync();

            return _mapper.Map<List<ProductSalesForecastDto>>(forecasts);
        }

        public async Task<bool> TTrainAndGenerateForecastsAsync()
        {
            var allOrderItems = await _orderItemRepository.GetAll()
                .Include(x => x.Order)
                .Where(x => !x.IsDeleted && !x.Order.IsDeleted)
                .ToListAsync();

            if (!allOrderItems.Any()) return false;

            var historicalData = allOrderItems
                .GroupBy(x => new
                {
                    x.ProductId,
                    x.Order.CreatedDate.Year,
                    x.Order.CreatedDate.Month
                })
                .Select(g => new ProductSalesModelInput
                {
                    ProductId = g.Key.ProductId.ToString(),
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    AveragePrice = (float)g.Average(i => i.PriceAtPurchase),
                    TotalQuantitySold = (float)g.Sum(i => i.Quantity)
                })
                .ToList();

            if (historicalData.Count < 10)
                throw new Exception("Makine öğrenmesi için yeterli geçmiş ürün satışı bulunmuyor.");

            IDataView dataView = _mlContext.Data.LoadFromEnumerable(historicalData);

            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding(
                    "ProductIdEncoded",
                    nameof(ProductSalesModelInput.ProductId))
                .Append(_mlContext.Transforms.Concatenate(
                    "Features",
                    "ProductIdEncoded",
                    nameof(ProductSalesModelInput.Year),
                    nameof(ProductSalesModelInput.Month),
                    nameof(ProductSalesModelInput.AveragePrice)))
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(_mlContext.Regression.Trainers.Sdca(
                    labelColumnName: "Label",
                    featureColumnName: "Features"));

            var model = pipeline.Fit(dataView);
            _mlContext.Model.Save(model, dataView.Schema, _modelPath);

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ProductSalesModelInput, ProductSalesModelOutput>(model);

            var lastOrderDate = allOrderItems.Max(x => x.Order.CreatedDate);
            var targetMonth = lastOrderDate.AddMonths(1).Month;
            var targetYear = lastOrderDate.AddMonths(1).Year;

            var activeProducts = await _productRepository.GetAll()
                .Where(x => !x.IsDeleted)
                .ToListAsync();

            foreach (var product in activeProducts)
            {
                var input = new ProductSalesModelInput
                {
                    ProductId = product.Id.ToString(),
                    Month = targetMonth,
                    Year = targetYear,
                    AveragePrice = (float)product.Price
                };

                var prediction = predictionEngine.Predict(input);

                int predictedQty = (int)Math.Round(prediction.PredictedQuantity);
                if (predictedQty < 0)
                    predictedQty = 0;

                decimal expectedRev = predictedQty * product.Price;

                var existingForecast = await _forecastRepo.GetAll()
                    .FirstOrDefaultAsync(x =>
                        x.ProductId == product.Id &&
                        x.TargetYear == targetYear &&
                        x.TargetMonth == targetMonth);

                if (existingForecast == null)
                {
                    await _forecastRepo.AddAsync(new ProductSalesForecast
                    {
                        ProductId = product.Id,
                        TargetMonth = targetMonth,
                        TargetYear = targetYear,
                        PredictedQuantity = predictedQty,
                        ExpectedRevenue = expectedRev,
                        ConfidenceScore = 0.88,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    });
                }
                else
                {
                    existingForecast.PredictedQuantity = predictedQty;
                    existingForecast.ExpectedRevenue = expectedRev;
                    existingForecast.UpdatedDate = DateTime.UtcNow;
                    _forecastRepo.Update(existingForecast);
                }
            }

            await _uow.SaveAsync();
            return true;
        }
    }
}
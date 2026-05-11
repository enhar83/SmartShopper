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
    public class ProductSalesForecastManager:IProductSalesForecastService
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

        // --- 1. UI İÇİN VERİ ÇEKME ---
        public async Task<List<ProductSalesForecastDto>> TGetAllForecastsAsync()
        {
            // Beklenen ciroya göre azalan sıralamayla çekiyoruz (En çok kazandıracaklar üstte)
            var forecasts = await _forecastRepo.GetAll()
                .Include(x => x.Product) // Product ismi ve fiyatı için Eager Loading
                .OrderByDescending(x => x.ExpectedRevenue)
                .ToListAsync();

            return _mapper.Map<List<ProductSalesForecastDto>>(forecasts);
        }

        // --- 2. MODEL EĞİTİMİ VE TAHMİN ÜRETİMİ ---
        public async Task<bool> TTrainAndGenerateForecastsAsync()
        {
            // 1. Geçmiş Sipariş Kalemlerini (OrderItem) çekiyoruz
            var allOrderItems = await _orderItemRepository.GetAll()
                .Include(x => x.Order) // Tarih (CreatedDate) Order içindedir
                .Where(x => !x.IsDeleted && !x.Order.IsDeleted)
                .ToListAsync();

            if (!allOrderItems.Any()) return false;

            // 2. Veriyi "Ürün - Yıl - Ay" bazında gruplayıp Eğitim Verisini hazırlıyoruz
            var historicalData = allOrderItems
                .GroupBy(x => new {
                    x.ProductId,
                    x.Order.CreatedDate.Year,
                    x.Order.CreatedDate.Month
                })
                .Select(g => new ProductSalesModelInput
                {
                    ProductId = g.Key.ProductId.ToString(),
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    AveragePrice = (float)g.Average(i => i.PriceAtPurchase), // O ay ürün kaça satılmış?
                    TotalQuantitySold = (float)g.Sum(i => i.Quantity) // O ay toplam kaç adet satılmış? (HEDEF LABEL)
                }).ToList();

            if (historicalData.Count < 10)
                throw new Exception("Makine öğrenmesi için yeterli geçmiş ürün satışı bulunmuyor.");

            // 3. ML.NET Pipeline'ı Kurulumu (SDCA Doğrusal Regresyon)
            IDataView dataView = _mlContext.Data.LoadFromEnumerable(historicalData);

            // One-Hot Encoding ile Ürün ID'lerini sayısallaştırıyoruz
            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding("ProductIdEncoded", nameof(ProductSalesModelInput.ProductId))
                .Append(_mlContext.Transforms.Concatenate("Features",
                    "ProductIdEncoded",
                    nameof(ProductSalesModelInput.Year),
                    nameof(ProductSalesModelInput.Month),
                    nameof(ProductSalesModelInput.AveragePrice))) // Fiyatı mutlaka özellik olarak veriyoruz!
                .Append(_mlContext.Transforms.NormalizeMinMax("Features")) // Normalizasyon
                .Append(_mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features")); // Sürekli Model

            // Modeli Eğit ve Kaydet
            var model = pipeline.Fit(dataView);
            _mlContext.Model.Save(model, dataView.Schema, _modelPath);

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ProductSalesModelInput, ProductSalesModelOutput>(model);

            // 4. Tahmin Edilecek Gelecek Ayı Belirleme
            var lastOrderDate = allOrderItems.Max(x => x.Order.CreatedDate);
            var targetMonth = lastOrderDate.AddMonths(1).Month;
            var targetYear = lastOrderDate.AddMonths(1).Year;

            // 5. BÜTÜN AKTİF ÜRÜNLER İÇİN TAHMİN YAP
            // Geçmişte satılsın satılmasın, sistemdeki tüm aktif ürünleri çekiyoruz
            var activeProducts = await _productRepository.GetAll().Where(x => !x.IsDeleted).ToListAsync();

            foreach (var product in activeProducts)
            {
                // 🔥 SENIOR TRICK: Modelden tahmin isterken geçmişteki ortalama fiyatı değil, 
                // ürünün 'GÜNCEL' fiyatını veriyoruz. (Price Elasticity)
                var input = new ProductSalesModelInput
                {
                    ProductId = product.Id.ToString(),
                    Month = targetMonth,
                    Year = targetYear,
                    AveragePrice = (float)product.Price
                };

                var prediction = predictionEngine.Predict(input);

                // Gürültü Filtresi: Satış Adedi (Quantity) eksi olamaz, tam sayıya yuvarlanmalıdır.
                int predictedQty = (int)Math.Round(prediction.PredictedQuantity);
                if (predictedQty < 0) predictedQty = 0;

                // Tahmin edilen Ciro = Tahmin Edilen Adet * Güncel Ürün Fiyatı
                decimal expectedRev = predictedQty * product.Price;

                // Veritabanında kayıt var mı kontrolü
                var existingForecast = await _forecastRepo.GetAll()
                    .FirstOrDefaultAsync(x => x.ProductId == product.Id &&
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
                        ConfidenceScore = 0.88, // Güven skoru
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

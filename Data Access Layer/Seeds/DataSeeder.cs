using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data_Access_Layer.DbContext;
using Data_Access_Layer.Seeds.ProductSeeds;
using EFCore.BulkExtensions;
using Entity_Layer;
using Microsoft.EntityFrameworkCore;

namespace Data_Access_Layer.Seeds
{
    public class DataSeeder
    {
        private readonly AppDbContext _context;
        private readonly SeedService _seedService;

        public DataSeeder(AppDbContext context)
        {
            _context = context;
            _seedService = new SeedService();
        }

        public async Task SeedOnlyProductsAsync()
        {
            // 1. DB'den alt kategorileri çek
            var existingSubCategories = await _context.SubCategories
                .Include(s => s.Category)
                .ToListAsync();

            if (existingSubCategories.Count == 0) return;

            // 2. Hangi kategorilerin ürünleri DB'de halihazırda var?
            // Ürünlerin bağlı olduğu SubCategory üzerinden Category isimlerini çekiyoruz.
            var existingProductCategories = await _context.Products
                .Select(p => p.SubCategory.Category.Name)
                .Distinct()
                .ToListAsync();

            List<Product> productsToInsert = new();

            // 3. TECHNOLOGY KONTROLÜ
            if (!existingProductCategories.Contains("Technology"))
            {
                Console.WriteLine(">> Technology ürünleri eksik, üretiliyor...");
                var techSeed = new TechnologySeed();
                productsToInsert.AddRange(techSeed.GetProducts(existingSubCategories));
            }

            // 4. COSMETICS & BEAUTY KONTROLÜ
            if (!existingProductCategories.Contains("Cosmetics & Beauty"))
            {
                Console.WriteLine(">> Cosmetics & Beauty ürünleri eksik, üretiliyor...");
                var beautySeed = new CosmeticsAndBeautySeed();
                productsToInsert.AddRange(beautySeed.GetProducts(existingSubCategories));
            }

            // 5. JEWELRY & ACCESSORIES KONTROLÜ
            if (!existingProductCategories.Contains("Jewelry & Accessories"))
            {
                Console.WriteLine(">> Jewelry & Accessories ürünleri eksik, üretiliyor...");
                var jewelrySeed = new JewelryAndAccessoriesSeed();
                productsToInsert.AddRange(jewelrySeed.GetProducts(existingSubCategories));
            }

            // 6. FOOTWEAR KONTROLÜ
            if (!existingProductCategories.Contains("Footwear"))
            {
                Console.WriteLine(">> Footwear ürünleri eksik, üretiliyor...");
                var footwearSeed = new FootwearSeed();
                productsToInsert.AddRange(footwearSeed.GetProducts(existingSubCategories));
            }

            // 7. GARDEN & PATIO KONTROLÜ
            if (!existingProductCategories.Contains("Garden & Patio"))
            {
                Console.WriteLine(">> Garden & Patio ürünleri eksik, üretiliyor...");
                var gardenSeed = new GardenAndPatioSeed();
                productsToInsert.AddRange(gardenSeed.GetProducts(existingSubCategories));
            }

            // Office & Stationery
            if (!existingProductCategories.Contains("Office & Stationery"))
            {
                var officeSeed = new OfficeAndStationerySeed();
                productsToInsert.AddRange(officeSeed.GetProducts(existingSubCategories));
            }

            // Automotive
            if (!existingProductCategories.Contains("Automotive"))
            {
                var autoSeed = new AutomotiveSeed();
                productsToInsert.AddRange(autoSeed.GetProducts(existingSubCategories));
            }

            // Supermarket
            if (!existingProductCategories.Contains("Supermarket"))
            {
                var marketSeed = new SupermarketSeed();
                productsToInsert.AddRange(marketSeed.GetProducts(existingSubCategories));
            }

            // Pet Supplies
            if (!existingProductCategories.Contains("Pet Supplies"))
            {
                var petSeed = new PetSuppliesSeed();
                productsToInsert.AddRange(petSeed.GetProducts(existingSubCategories));
            }

            // Home & Furniture
            if (!existingProductCategories.Contains("Home & Furniture"))
            {
                var homeSeed = new HomeAndFurnitureSeed();
                productsToInsert.AddRange(homeSeed.GetProducts(existingSubCategories));
            }

            // Books & Media
            if (!existingProductCategories.Contains("Books & Media"))
            {
                var bookSeed = new BooksAndMediaSeed();
                productsToInsert.AddRange(bookSeed.GetProducts(existingSubCategories));
            }

            // Fashion & Apparel
            if (!existingProductCategories.Contains("Fashion & Apparel"))
            {
                var fashionSeed = new FashionAndApparelSeed();
                productsToInsert.AddRange(fashionSeed.GetProducts(existingSubCategories));
            }

            // Baby & Kids
            if (!existingProductCategories.Contains("Baby & Kids"))
            {
                var babySeed = new BabyAndKidsSeed();
                productsToInsert.AddRange(babySeed.GetProducts(existingSubCategories));
            }

            // Sports & Outdoor
            if (!existingProductCategories.Contains("Sports & Outdoor"))
            {
                var sportSeed = new SportsAndOutdoorSeed();
                productsToInsert.AddRange(sportSeed.GetProducts(existingSubCategories));
            }

            // Toys & Hobby
            if (!existingProductCategories.Contains("Toys & Hobby"))
            {
                var toySeed = new ToysAndHobbySeed();
                productsToInsert.AddRange(toySeed.GetProducts(existingSubCategories));
            }

            Console.WriteLine("Existing Product Categories:");

            foreach (var item in existingProductCategories)
            {
                Console.WriteLine(item);
            }

            if (productsToInsert.Count > 0)
            {
                foreach (var p in productsToInsert) p.SubCategory = null!;

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        await _context.BulkInsertAsync(productsToInsert, config =>
                        {
                            config.PreserveInsertOrder = true;
                            config.BatchSize = 5000;
                        });
                        await transaction.CommitAsync();
                        Console.WriteLine($">> BAŞARILI: {productsToInsert.Count} yeni ürün eklendi.");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception("Seed sırasında hata oluştu!", ex);
                    }
                }
            }
            else
            {
                Console.WriteLine(">> Tüm kategoriler güncel, yeni ürün eklenmedi.");
            }
        }
    }
}
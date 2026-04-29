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

            // 5. TOPLU EKLEME İŞLEMİ
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
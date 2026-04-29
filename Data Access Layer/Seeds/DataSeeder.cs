using EFCore.BulkExtensions;
using Data_Access_Layer.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            // 1. DB'den mevcut alt kategorileri Category bilgisiyle birlikte çekiyoruz
            // .Include(s => s.Category) kısmı kritik, çünkü TechnologySeed bu isme bakar.
            var existingSubCategories = await _context.SubCategories
                .Include(s => s.Category)
                .ToListAsync();

            if (existingSubCategories.Count == 0)
            {
                Console.WriteLine(">> HATA: Veritabanında alt kategori bulunamadı.");
                return;
            }

            // 2. Modüler yeni metodumuzu çağırıyoruz
            // Bu metod içeride TechnologySeed'i (ve ekleyeceğin diğerlerini) çalıştırır.
            _seedService.SeedAllProducts(existingSubCategories);
            var products = _seedService.GeneratedProducts;

            if (products.Count == 0)
            {
                Console.WriteLine(">> UYARI: Üretilecek ürün bulunamadı (Filtreleme kriterlerini kontrol edin).");
                return;
            }

            // 3. İlişki temizliği (Bulk Insert sırasında Foreign Key hatalarını önlemek için)
            foreach (var p in products)
            {
                p.SubCategory = null!;
            }

            // 4. Veritabanına Toplu Yazma İşlemi
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.BulkInsertAsync(products, config =>
                    {
                        config.PreserveInsertOrder = true;
                        config.SetOutputIdentity = false;
                        config.BatchSize = 5000;
                    });

                    await transaction.CommitAsync();
                    Console.WriteLine($">> BAŞARILI: {products.Count} yeni ürün (Technology vb.) DB'ye eklendi.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    // İç hatayı (InnerException) görmek hata ayıklama için önemlidir
                    throw new Exception($"Ürün Seed Hatası! Detay: {ex.InnerException?.Message ?? ex.Message}", ex);
                }
            }
        }
    }
}
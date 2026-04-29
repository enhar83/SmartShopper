using System;
using System.Collections.Generic;
using System.Linq;
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

        public DataSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedOnlyProductsAsync()
        {
            var existingSubCategories = await _context.SubCategories
                .Include(s => s.Category)
                .ToListAsync();

            if (existingSubCategories.Count == 0) return;

            var existingProductCategories = await _context.Products
                .Select(p => p.SubCategory.Category.Name)
                .Distinct()
                .ToListAsync();

            List<Product> productsToInsert = new();

            // Kategori Kontrolleri (Örnek mantık akışı)
            if (!existingProductCategories.Contains("Technology"))
                productsToInsert.AddRange(new TechnologySeed().GetProducts(existingSubCategories));

            if (!existingProductCategories.Contains("Automotive"))
                productsToInsert.AddRange(new AutomotiveSeed().GetProducts(existingSubCategories));

            if (!existingProductCategories.Contains("Footwear"))
                productsToInsert.AddRange(new FootwearSeed().GetProducts(existingSubCategories));

            // ... Diğer tohumlama sınıfları buraya eklenebilir ...

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
        }
    }
}
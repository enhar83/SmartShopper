using Entity_Layer;
using System;
using System.Collections.Generic;
using Data_Access_Layer.Seeds.ProductSeeds; // TechnologySeed'e erişmek için

namespace Data_Access_Layer.Seeds
{
    public class SeedService
    {
        // Sadece üretilen ürünleri tutan liste
        public List<Product> GeneratedProducts { get; private set; } = new List<Product>();

        /// <summary>
        /// Tüm kategoriler için ürün üretimini başlatır.
        /// </summary>
        /// <param name="existingSubCategories">DB'den gelen tüm alt kategoriler (Category dahil edilmeli)</param>
        public void SeedAllProducts(List<SubCategory> existingSubCategories)
        {
            GeneratedProducts.Clear();

            // 1. TECHNOLOGY ÜRÜNLERİNİ ÜRET
            var techSeed = new TechnologySeed();
            var techProducts = techSeed.GetProducts(existingSubCategories);
            GeneratedProducts.AddRange(techProducts);

            // 2. İLERİDE DİĞER KATEGORİLER BURAYA EKLENECEK
            // var clothingSeed = new ClothingSeed();
            // GeneratedProducts.AddRange(clothingSeed.GetProducts(existingSubCategories));

            // ... diğer kategori seedleri ...
        }

        // Eski metodunu ihtiyacın olursa diye aşağıda basit bir fallback olarak tutabilirsin 
        // ya da tamamen silebilirsin.
    }
}
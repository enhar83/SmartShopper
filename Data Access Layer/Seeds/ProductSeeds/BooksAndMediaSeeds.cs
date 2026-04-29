using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Entity_Layer;
using static Data_Access_Layer.Seeds.ProductSeeds.TechnologySeed;

namespace Data_Access_Layer.Seeds.ProductSeeds
{
    public class BooksAndMediaSeed
    {
        private readonly Faker _faker = new Faker("en");

        private readonly Dictionary<string, SubCategoryRule> _rules = new()
        {
            ["Fiction"] = new() { MinPrice = 40, MaxPrice = 350, MinProducts = 30, MaxProducts = 40, Names = new[] { "The Silent Echo", "Beneath the Crimson Sky", "Beyond the Horizon", "The Last Detective" } },
            ["Non-Fiction"] = new() { MinPrice = 60, MaxPrice = 850, MinProducts = 20, MaxProducts = 30, Names = new[] { "Mastering Modern Minds", "History of the Lost Cities", "The Science of Habit", "Financial Freedom Roadmap" } },
            ["Self-Help"] = new() { MinPrice = 50, MaxPrice = 450, MinProducts = 20, MaxProducts = 30, Names = new[] { "Atomic Growth", "Mindful Living Guide", "Unlocking Your Potential", "The Power of Resilience" } },
            ["Children Books"] = new() { MinPrice = 30, MaxPrice = 250, MinProducts = 10, MaxProducts = 20, Names = new[] { "The Brave Little Fox", "Adventure in Space", "Magic Forest Tales", "Bedtime Animal Stories" } },
            ["Biographies"] = new() { MinPrice = 70, MaxPrice = 600, MinProducts = 5, MaxProducts = 15, Names = new[] { "A Life of Innovation", "Behind the Screen", "The Path to Victory", "Architect of Dreams" } },
            ["Comics"] = new() { MinPrice = 45, MaxPrice = 1500, MinProducts = 3, MaxProducts = 7, Names = new[] { "Galactic Guardians #1", "Night Shadow: Origins", "Tales from the Deep", "Supernova Justice" } }
        };

        public List<Product> GetProducts(List<SubCategory> allSubCategories)
        {
            var bookSubs = allSubCategories.Where(x => x.CategoryId == Guid.Parse("536599C7-4E92-46A6-83E8-0926BFD03E5D")).ToList();
            var products = new List<Product>();

            foreach (var sub in bookSubs)
            {
                if (!_rules.TryGetValue(sub.Name, out var rule)) continue;
                for (int i = 0; i < _faker.Random.Int(rule.MinProducts, rule.MaxProducts); i++)
                {
                    var baseName = _faker.PickRandom(rule.Names);
                    products.Add(new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = baseName, // Kitaplarda genelde doğrudan isim kullanılır
                        Description = $"Dive into the pages of '{baseName}'. A compelling read that offers unique insights and an unforgettable journey through its unique narrative.",
                        Price = _faker.Finance.Amount(rule.MinPrice, rule.MaxPrice),
                        Stock = _faker.Random.Int(50, 2000),
                        SubCategoryId = sub.Id,
                        Gender = null,
                        CreatedDate = sub.CreatedDate.AddDays(_faker.Random.Int(1, 120))
                    });
                }
            }
            return products;
        }
    }
}
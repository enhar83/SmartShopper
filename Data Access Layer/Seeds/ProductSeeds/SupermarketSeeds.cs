using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Entity_Layer;
using static Data_Access_Layer.Seeds.ProductSeeds.TechnologySeed;

namespace Data_Access_Layer.Seeds.ProductSeeds
{
    public class SupermarketSeed
    {
        private readonly Faker _faker = new Faker("en");

        private readonly Dictionary<string, SubCategoryRule> _rules = new()
        {
            ["Beverages"] = new() { MinPrice = 15, MaxPrice = 450, MinProducts = 30, MaxProducts = 50, Names = new[] { "Cold Brew Coffee", "Organic Orange Juice", "Sparkling Spring Water", "Energy Vitamin Drink", "Herbal Tea Blend" } },
            ["Dairy"] = new() { MinPrice = 30, MaxPrice = 800, MinProducts = 20, MaxProducts = 40, Names = new[] { "Full Fat Greek Yogurt", "Aged Cheddar Cheese", "Almond Milk Unsweetened", "Grass-Fed Butter", "Fresh Goat Cheese" } },
            ["Snacks"] = new() { MinPrice = 10, MaxPrice = 250, MinProducts = 20, MaxProducts = 40, Names = new[] { "Roasted Salted Almonds", "Dark Chocolate Sea Salt", "Baked Potato Crisps", "Organic Fruit Bar" } },
            ["Frozen Food"] = new() { MinPrice = 80, MaxPrice = 1500, MinProducts = 10, MaxProducts = 20, Names = new[] { "Frozen Wild Blueberries", "Artisan Thin Crust Pizza", "Vegetable Spring Rolls", "Atlantic Salmon Fillets" } },
            ["Cleaning"] = new() { MinPrice = 45, MaxPrice = 1200, MinProducts = 8, MaxProducts = 12, Names = new[] { "Multi-Surface Eco Cleaner", "Lemon Scent Dish Soap", "Concentrated Laundry Pods", "Glass & Mirror Spray" } }
        };

        public List<Product> GetProducts(List<SubCategory> allSubCategories)
        {
            var marketSubs = allSubCategories.Where(x => x.CategoryId == Guid.Parse("F6160144-5ACE-47FE-A83B-75A1C3835E6D")).ToList();
            var products = new List<Product>();

            foreach (var sub in marketSubs)
            {
                if (!_rules.TryGetValue(sub.Name, out var rule)) continue;
                for (int i = 0; i < _faker.Random.Int(rule.MinProducts, rule.MaxProducts); i++)
                {
                    var baseName = _faker.PickRandom(rule.Names);
                    products.Add(new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = baseName, // Supermarket ürünlerinde marka genellikle ismin içindedir veya sade tercih edilir
                        Description = $"Fresh and high-quality {baseName}. Sourced from trusted producers to ensure the best taste and hygiene for your family.",
                        Price = _faker.Finance.Amount(rule.MinPrice, rule.MaxPrice),
                        Stock = _faker.Random.Int(100, 5000), // Süpermarket stokları genelde çok yüksektir
                        SubCategoryId = sub.Id,
                        Gender = null,
                        CreatedDate = sub.CreatedDate.AddDays(_faker.Random.Int(1, 15))
                    });
                }
            }
            return products;
        }
    }
}
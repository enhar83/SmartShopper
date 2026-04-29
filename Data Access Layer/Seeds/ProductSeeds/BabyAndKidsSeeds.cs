using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Entity_Layer;
using static Data_Access_Layer.Seeds.ProductSeeds.TechnologySeed;

namespace Data_Access_Layer.Seeds.ProductSeeds
{
    public class BabyAndKidsSeed
    {
        private readonly Faker _faker = new Faker("en");

        private readonly Dictionary<string, SubCategoryRule> _rules = new()
        {
            ["Nursery"] = new() { MinPrice = 1500, MaxPrice = 25000, MinProducts = 5, MaxProducts = 10, Names = new[] { "Convertible Baby Crib", "Wooden Changing Table", "Glider Nursing Chair", "Breathable Crib Mattress" } },
            ["Nursing"] = new() { MinPrice = 200, MaxPrice = 4500, MinProducts = 5, MaxProducts = 10, Names = new[] { "Electric Breast Pump", "Nursing Pillow", "Sterilizer & Dryer", "Multi-Pack Baby Bottles" } },
            ["Strollers"] = new() { MinPrice = 3500, MaxPrice = 45000, MinProducts = 6, MaxProducts = 9, Names = new[] { "All-Terrain Travel System", "Lightweight Compact Stroller", "Double Jogging Stroller", "Luxury Bassinet Combo" } },
            ["Diapering"] = new() { MinPrice = 100, MaxPrice = 1500, MinProducts = 6, MaxProducts = 9, Names = new[] { "Eco-Friendly Bamboo Diapers", "Portable Changing Pad", "Diaper Pail Refills", "Organic Sensitive Wipes" } },
            ["Feeding"] = new() { MinPrice = 150, MaxPrice = 3500, MinProducts = 7, MaxProducts = 10, Names = new[] { "High Chair with Tray", "Silicone Suction Bowl Set", "Training Cup with Straw", "Organic Baby Food Maker" } },
            ["Safety"] = new() { MinPrice = 300, MaxPrice = 8500, MinProducts = 5, MaxProducts = 8, Names = new[] { "Smart Video Baby Monitor", "Auto-Close Safety Gate", "Corner Guard Protectors", "Car Seat Back Mirror" } },
            ["Clothing"] = new() { MinPrice = 200, MaxPrice = 3500, MinProducts = 15, MaxProducts = 20, Names = new[] { "Organic Cotton Onesie Pack", "Toddler Hooded Jacket", "Baby Sleeping Sack", "Soft Knit Booties" } }
        };

        public List<Product> GetProducts(List<SubCategory> allSubCategories)
        {
            var babySubs = allSubCategories.Where(x => x.CategoryId == Guid.Parse("37B0FF25-198B-4D1B-9DC9-40BDD5C0EBF5")).ToList();
            var products = new List<Product>();

            foreach (var sub in babySubs)
            {
                if (!_rules.TryGetValue(sub.Name, out var rule)) continue;
                for (int i = 0; i < _faker.Random.Int(rule.MinProducts, rule.MaxProducts); i++)
                {
                    var baseName = _faker.PickRandom(rule.Names);
                    products.Add(new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = $"{_faker.Company.CompanyName()} {baseName}",
                        Description = $"Carefully designed for your little one's safety and comfort. This {baseName} uses non-toxic, baby-safe materials for total peace of mind.",
                        Price = _faker.Finance.Amount(rule.MinPrice, rule.MaxPrice),
                        Stock = _faker.Random.Int(10, 300),
                        SubCategoryId = sub.Id,
                        Gender = _faker.PickRandom(GenderType.Male, GenderType.Female, GenderType.Unisex),
                        CreatedDate = sub.CreatedDate.AddDays(_faker.Random.Int(1, 30))
                    });
                }
            }
            return products;
        }
    }
}
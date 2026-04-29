using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Entity_Layer;
using static Data_Access_Layer.Seeds.ProductSeeds.TechnologySeed;

namespace Data_Access_Layer.Seeds.ProductSeeds
{
    public class SportsAndOutdoorSeed
    {
        private readonly Faker _faker = new Faker("en");

        private readonly Dictionary<string, SubCategoryRule> _rules = new()
        {
            ["Fitness"] = new() { MinPrice = 150, MaxPrice = 25000, MinProducts = 20, MaxProducts = 30, Names = new[] { "Adjustable Dumbbell Set", "Yoga Mat Pro", "Magnetic Resistance Rowing Machine", "Kettlebell Series", "Resistance Band Kit" } },
            ["Cycling"] = new() { MinPrice = 1500, MaxPrice = 65000, MinProducts = 10, MaxProducts = 20, Names = new[] { "Mountain Bike 29er", "Carbon Fiber Road Bike", "Electric Commuter Bike", "Folding City Cycle", "Kids' Safety Bicycle" } },
            ["Winter Sports"] = new() { MinPrice = 800, MaxPrice = 15000, MinProducts = 10, MaxProducts = 20, Names = new[] { "All-Mountain Snowboard", "Ski Goggles Anti-Fog", "Insulated Winter Boots", "Thermal Ski Jacket" } },
            ["Camping"] = new() { MinPrice = 500, MaxPrice = 12000, MinProducts = 5, MaxProducts = 10, Names = new[] { "4-Person Instant Tent", "Sleeping Bag -10C", "Portable Camping Stove", "Rechargeable LED Lantern" } },
            ["Water Sports"] = new() { MinPrice = 300, MaxPrice = 18000, MinProducts = 6, MaxProducts = 10, Names = new[] { "Inflatable Stand Up Paddleboard", "Professional Kayak", "Snorkel Mask Set", "Quick-Dry Wetsuit" } },
            ["Hiking"] = new() { MinPrice = 400, MaxPrice = 8500, MinProducts = 4, MaxProducts = 7, Names = new[] { "Telescopic Trekking Poles", "50L Expedition Backpack", "Waterproof Trail Map Case", "Hydration Bladder" } },
            ["Team Sports"] = new() { MinPrice = 100, MaxPrice = 4500, MinProducts = 15, MaxProducts = 25, Names = new[] { "Official Size Basketball", "Professional Soccer Ball", "Aluminum Baseball Bat", "Tennis Racket Pro" } }
        };

        public List<Product> GetProducts(List<SubCategory> allSubCategories)
        {
            var sportSubs = allSubCategories.Where(x => x.CategoryId == Guid.Parse("A997D8AC-A157-4FA9-974D-4EE1E5B45898")).ToList();
            var products = new List<Product>();

            foreach (var sub in sportSubs)
            {
                if (!_rules.TryGetValue(sub.Name, out var rule)) continue;
                for (int i = 0; i < _faker.Random.Int(rule.MinProducts, rule.MaxProducts); i++)
                {
                    var baseName = _faker.PickRandom(rule.Names);
                    products.Add(new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = $"{_faker.Company.CompanyName()} {baseName}",
                        Description = $"Elevate your performance with this {baseName}. Engineered for athletes who demand high durability and peak functionality in any environment.",
                        Price = _faker.Finance.Amount(rule.MinPrice, rule.MaxPrice),
                        Stock = _faker.Random.Int(5, 200),
                        SubCategoryId = sub.Id,
                        Gender = _faker.PickRandom(GenderType.Male, GenderType.Female, GenderType.Unisex),
                        CreatedDate = sub.CreatedDate.AddDays(_faker.Random.Int(1, 60))
                    });
                }
            }
            return products;
        }
    }
}
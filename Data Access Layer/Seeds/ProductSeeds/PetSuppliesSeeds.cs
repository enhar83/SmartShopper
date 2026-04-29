using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Entity_Layer;
using static Data_Access_Layer.Seeds.ProductSeeds.TechnologySeed;

namespace Data_Access_Layer.Seeds.ProductSeeds
{
    public class PetSuppliesSeed
    {
        private readonly Faker _faker = new Faker("en");

        private readonly Dictionary<string, SubCategoryRule> _rules = new()
        {
            ["Dog Food"] = new() { MinPrice = 200, MaxPrice = 3500, MinProducts = 10, MaxProducts = 20, Names = new[] { "Grain-Free Salmon & Potato", "High Protein Puppy Kibble", "Senior Care Lamb Formula", "Organic Beef Bites" } },
            ["Cat Food"] = new() { MinPrice = 150, MaxPrice = 2800, MinProducts = 5, MaxProducts = 15, Names = new[] { "Indoor Cat Turkey Mix", "Ocean Fish Wet Food Multipack", "Kitten Growth Formula", "Weight Control Chicken" } },
            ["Toys"] = new() { MinPrice = 50, MaxPrice = 1200, MinProducts = 3, MaxProducts = 13, Names = new[] { "Interactive Laser Toy", "Durable Rubber Chew Bone", "Plush Squeaky Squirrel", "Feather Teaser Wand", "Treat Dispensing Ball" } },
            ["Grooming"] = new() { MinPrice = 100, MaxPrice = 2500, MinProducts = 5, MaxProducts = 15, Names = new[] { "Self-Cleaning Slicker Brush", "Organic Oatmeal Shampoo", "Electric Nail Grinder", "De-shedding Undercoat Tool" } },
            ["Aquarium"] = new() { MinPrice = 300, MaxPrice = 15000, MinProducts = 3, MaxProducts = 5, Names = new[] { "LED Lighted Fish Tank", "Silent Power Filter", "Decorative Coral Reef Ornament", "Automatic Fish Feeder" } },
            ["Health"] = new() { MinPrice = 80, MaxPrice = 2000, MinProducts = 2, MaxProducts = 5, Names = new[] { "Hip & Joint Supplements", "Probiotic Digestive Support", "Calming Hemp Chews", "Multivitamin Soft Chews" } },
            ["Small Pet Care"] = new() { MinPrice = 60, MaxPrice = 1500, MinProducts = 1, MaxProducts = 3, Names = new[] { "Timothy Hay Timothy", "Wooden Hamster Wheel", "Rabbit Hutch Liner", "Guinea Pig Hideout" } }
        };

        public List<Product> GetProducts(List<SubCategory> allSubCategories)
        {
            var petSubs = allSubCategories.Where(x => x.CategoryId == Guid.Parse("B0DA68FC-6ADB-4966-8706-74F0DDFF0418")).ToList();
            var products = new List<Product>();

            foreach (var sub in petSubs)
            {
                if (!_rules.TryGetValue(sub.Name, out var rule)) continue;
                for (int i = 0; i < _faker.Random.Int(rule.MinProducts, rule.MaxProducts); i++)
                {
                    var baseName = _faker.PickRandom(rule.Names);
                    products.Add(new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = $"{_faker.Company.CompanyName()} {baseName}",
                        Description = $"{baseName} crafted with the highest safety standards. Ensuring your furry friend stays healthy, active, and happy every single day.",
                        Price = _faker.Finance.Amount(rule.MinPrice, rule.MaxPrice),
                        Stock = _faker.Random.Int(20, 800),
                        SubCategoryId = sub.Id,
                        Gender = null,
                        CreatedDate = sub.CreatedDate.AddDays(_faker.Random.Int(1, 45))
                    });
                }
            }
            return products;
        }
    }
}
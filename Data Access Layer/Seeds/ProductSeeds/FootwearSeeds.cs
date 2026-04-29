using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Entity_Layer;
using static Data_Access_Layer.Seeds.ProductSeeds.TechnologySeed;

namespace Data_Access_Layer.Seeds.ProductSeeds
{
    public class FootwearSeed
    {
        private readonly Faker _faker = new Faker("en");

        private readonly Dictionary<string, SubCategoryRule> _rules = new()
        {
            ["Sneakers"] = new()
            {
                MinPrice = 1200,
                MaxPrice = 18000,
                MinProducts = 22,
                MaxProducts = 35,
                Names = new[] { "Air Max Series", "Retro Low-Top", "Urban Street Runner", "Cloud Walk Foam", "Classic Canvas Sneaker", "Performance Knit Runner" }
            },

            ["Boots"] = new()
            {
                MinPrice = 2500,
                MaxPrice = 15000,
                MinProducts = 17,
                MaxProducts = 27,
                Names = new[] { "Waterproof Hiking Boots", "Classic Chelsea Leather", "Combat Lace-up", "Winter Insulated Boot", "Elegant Ankle Boot", "Western Style Suede" }
            },

            ["Formal"] = new()
            {
                MinPrice = 2000,
                MaxPrice = 12000,
                MinProducts = 15,
                MaxProducts = 25,
                Names = new[] { "Oxford Leather Shoes", "Classic Derby", "Polished Loafers", "Pointed Toe Stilettos", "Elegant Evening Pumps", "Patent Leather Brogues" }
            },

            ["Sandals"] = new()
            {
                MinPrice = 400,
                MaxPrice = 5000,
                MinProducts = 13,
                MaxProducts = 23,
                Names = new[] { "Ergonomic Slide", "Leather Gladiator Sandal", "Beach Flip-Flops", "Platform Wedge", "Active Outdoor Sandal" }
            },

            ["Sport Shoes"] = new()
            {
                MinPrice = 1500,
                MaxPrice = 9000,
                MinProducts = 18,
                MaxProducts = 28,
                Names = new[] { "Professional Football Cleats", "Basketball High-Tops", "Indoor Court Shoe", "Trail Running Series", "Gym Training Shoes" }
            },

            ["Slippers"] = new()
            {
                MinPrice = 150,
                MaxPrice = 2500,
                MinProducts = 7,
                MaxProducts = 17,
                Names = new[] { "Memory Foam Home Slipper", "Plush Fur Slides", "Orthopedic House Shoe", "Cotton Indoor Mules" }
            },

            ["Casual"] = new()
            {
                MinPrice = 800,
                MaxPrice = 6000,
                MinProducts = 5,
                MaxProducts = 15,
                Names = new[] { "Daily Slip-on", "Suede Boat Shoe", "Comfort Walking Loafer", "Lifestyle Flat Shoe" }
            }
        };

        public List<Product> GetProducts(List<SubCategory> allSubCategories)
        {
            var products = new List<Product>();

            // Footwear Category ID: CF83BCD1-6202-47FD-9A20-A6A4FF303702
            var footwearSubs = allSubCategories
                .Where(x => x.CategoryId == Guid.Parse("CF83BCD1-6202-47FD-9A20-A6A4FF303702"))
                .ToList();

            foreach (var sub in footwearSubs)
            {
                if (!_rules.TryGetValue(sub.Name, out var rule)) continue;

                int count = _faker.Random.Int(rule.MinProducts, rule.MaxProducts);

                for (int i = 0; i < count; i++)
                {
                    var baseName = _faker.PickRandom(rule.Names);
                    var brand = _faker.Company.CompanyName();

                    // Önce cinsiyeti belirle, sonra isme ekle (Daha gerçekçi görünür)
                    var gender = DetermineGender(sub.Name, baseName);
                    string genderPrefix = gender == GenderType.Male ? "Men's" : gender == GenderType.Female ? "Women's" : "Unisex";

                    string name = $"{brand} {genderPrefix} {baseName}";
                    string description = GenerateFootwearDescription(sub.Name, baseName);

                    products.Add(new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Description = description,
                        Price = _faker.Finance.Amount(rule.MinPrice, rule.MaxPrice),
                        Stock = _faker.Random.Int(15, 500),
                        SubCategoryId = sub.Id,
                        Gender = gender,
                        CreatedDate = sub.CreatedDate.AddDays(_faker.Random.Int(1, 60))
                    });
                }
            }
            return products;
        }

        private string GenerateFootwearDescription(string subCategory, string productName)
        {
            string[] features = { "Designed for ultimate comfort", "Featuring breathable materials", "Built with a durable outsole", "Engineered for maximum support", "Crafted with premium leather" };
            string[] tech = { "cushioning technology", "anti-slip grip", "memory foam insoles", "weather-resistant coating", "flexible fit system" };
            string[] useCase = { "perfect for everyday wear", "ideal for long-distance walking", "best suited for formal events", "great for intense workout sessions", "a must-have for the new season" };

            return $"{_faker.PickRandom(features)} and {_faker.PickRandom(tech)}. This {productName} is {_faker.PickRandom(useCase)}.";
        }

        private GenderType? DetermineGender(string subName, string productName)
        {
            string lowerName = productName.ToLower();

            // Kadınlara özel modeller
            if (lowerName.Contains("stilettos") || lowerName.Contains("pumps") || lowerName.Contains("wedge") || lowerName.Contains("ballerina"))
                return GenderType.Female;

            // Erkek odaklı olabilecek modeller (Genelleme)
            if (lowerName.Contains("oxford") || lowerName.Contains("derby") || lowerName.Contains("cleats"))
                return _faker.PickRandom(GenderType.Male, GenderType.Unisex);

            // Rastgele dağılım (Sneakers, Boots gibi kategoriler her iki cinsiyet için de üretilmeli)
            return _faker.PickRandom(GenderType.Male, GenderType.Female, GenderType.Unisex);
        }
    }
}
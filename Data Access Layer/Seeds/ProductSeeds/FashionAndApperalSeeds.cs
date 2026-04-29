using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Entity_Layer;
using static Data_Access_Layer.Seeds.ProductSeeds.TechnologySeed;

namespace Data_Access_Layer.Seeds.ProductSeeds
{
    public class FashionAndApparelSeed
    {
        private readonly Faker _faker = new Faker("en");

        private readonly Dictionary<string, SubCategoryRule> _rules = new()
        {
            ["Activewear"] = new() { MinPrice = 450, MaxPrice = 5500, MinProducts = 20, MaxProducts = 30, Names = new[] { "Compression Leggings", "Dry-Fit Training Tee", "Seamless Sports Bra", "High-Performance Tracksuit", "Thermal Base Layer" } },
            ["T-Shirts"] = new() { MinPrice = 250, MaxPrice = 1800, MinProducts = 40, MaxProducts = 50, Names = new[] { "Oversized Graphic Tee", "Classic V-Neck Cotton", "Slim Fit Essential", "Vintage Wash Crewneck" } },
            ["Suits"] = new() { MinPrice = 4500, MaxPrice = 35000, MinProducts = 10, MaxProducts = 20, Names = new[] { "Slim Fit 3-Piece Suit", "Classic Tuxedo", "Modern Tailored Blazer", "Wool Blend Formal Suit" } },
            ["Jackets"] = new() { MinPrice = 1500, MaxPrice = 25000, MinProducts = 10, MaxProducts = 20, Names = new[] { "Genuine Leather Biker Jacket", "Insulated Puffer Coat", "Classic Denim Jacket", "Lightweight Windbreaker" } },
            ["Jeans"] = new() { MinPrice = 800, MaxPrice = 6500, MinProducts = 8, MaxProducts = 12, Names = new[] { "High-Waist Skinny Jeans", "Relaxed Fit Straight Leg", "Distressed Slim Denim", "Classic Bootcut Jeans" } },
            ["Dresses"] = new() { MinPrice = 900, MaxPrice = 15000, MinProducts = 5, MaxProducts = 15, Names = new[] { "Floral Summer Maxi", "Elegant Little Black Dress", "Satin Evening Gown", "Boho Midi Dress" } },
            ["Knitwear"] = new() { MinPrice = 600, MaxPrice = 7500, MinProducts = 2, MaxProducts = 8, Names = new[] { "Cashmere Crewneck Sweater", "Oversized Chunky Cardigan", "Turtle Neck Knit", "Lightweight V-Neck Pullover" } },
            ["Swimwear"] = new() { MinPrice = 400, MaxPrice = 4500, MinProducts = 1, MaxProducts = 4, Names = new[] { "One-Piece Swimsuit", "Quick-Dry Board Shorts", "Classic Bikini Set", "Sporty Swim Trunks" } }
        };

        public List<Product> GetProducts(List<SubCategory> allSubCategories)
        {
            var fashionSubs = allSubCategories.Where(x => x.CategoryId == Guid.Parse("BBBAD97F-BE5C-456A-A95F-2CB2E6F54CDC")).ToList();
            var products = new List<Product>();

            foreach (var sub in fashionSubs)
            {
                if (!_rules.TryGetValue(sub.Name, out var rule)) continue;
                for (int i = 0; i < _faker.Random.Int(rule.MinProducts, rule.MaxProducts); i++)
                {
                    var baseName = _faker.PickRandom(rule.Names);
                    var gender = sub.Name == "Dresses" ? GenderType.Female : _faker.PickRandom(GenderType.Male, GenderType.Female, GenderType.Unisex);
                    var brand = _faker.Company.CompanyName();

                    products.Add(new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = $"{brand} {baseName}",
                        Description = $"This {baseName} combines seasonal trends with premium comfort. Made with high-quality fabric, it's a perfect addition to your stylish wardrobe.",
                        Price = _faker.Finance.Amount(rule.MinPrice, rule.MaxPrice),
                        Stock = _faker.Random.Int(20, 500),
                        SubCategoryId = sub.Id,
                        Gender = gender,
                        CreatedDate = sub.CreatedDate.AddDays(_faker.Random.Int(1, 45))
                    });
                }
            }
            return products;
        }
    }
}
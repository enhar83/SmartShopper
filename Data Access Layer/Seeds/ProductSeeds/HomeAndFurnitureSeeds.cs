using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Entity_Layer;
using static Data_Access_Layer.Seeds.ProductSeeds.TechnologySeed;

namespace Data_Access_Layer.Seeds.ProductSeeds
{
    public class HomeAndFurnitureSeed
    {
        private readonly Faker _faker = new Faker("en");

        private readonly Dictionary<string, SubCategoryRule> _rules = new()
        {
            ["Living Room"] = new() { MinPrice = 5000, MaxPrice = 85000, MinProducts = 5, MaxProducts = 15, Names = new[] { "Velvet 3-Seater Sofa", "Modern Marble Coffee Table", "Minimalist TV Stand", "Wingback Accent Chair" } },
            ["Bedroom"] = new() { MinPrice = 4000, MaxPrice = 65000, MinProducts = 5, MaxProducts = 15, Names = new[] { "Orthopedic Queen Mattress", "Upholstered Bed Frame", "Scandi-Style Nightstand", "6-Drawer Dresser" } },
            ["Kitchen"] = new() { MinPrice = 200, MaxPrice = 18000, MinProducts = 7, MaxProducts = 17, Names = new[] { "Non-Stick Ceramic Cookware Set", "Professional Chef Knife Kit", "Digital Air Fryer", "Adjustable Bar Stool" } },
            ["Decoration"] = new() { MinPrice = 100, MaxPrice = 8000, MinProducts = 3, MaxProducts = 9, Names = new[] { "Abstract Canvas Wall Art", "Decorative Floor Vase", "Aromatic Candle Set", "Boho Style Throw Rug" } },
            ["Lighting"] = new() { MinPrice = 150, MaxPrice = 12000, MinProducts = 2, MaxProducts = 5, Names = new[] { "Modern Sputnik Chandelier", "Adjustable Floor Lamp", "Dimmable Bedside Light", "Industrial Pendant Lamp" } },
            ["Bathroom"] = new() { MinPrice = 80, MaxPrice = 9500, MinProducts = 30, MaxProducts = 6, Names = new[] { "Bamboo Bath Mat", "Luxury Towel Set", "Wall-Mounted Mirror Cabinet", "Rainfall Shower Head" } }
        };

        public List<Product> GetProducts(List<SubCategory> allSubCategories)
        {
            var homeSubs = allSubCategories.Where(x => x.CategoryId == Guid.Parse("55124A75-2F1E-4908-8FBF-107498950E0A")).ToList();
            var products = new List<Product>();

            foreach (var sub in homeSubs)
            {
                if (!_rules.TryGetValue(sub.Name, out var rule)) continue;
                for (int i = 0; i < _faker.Random.Int(rule.MinProducts, rule.MaxProducts); i++)
                {
                    var baseName = _faker.PickRandom(rule.Names);
                    products.Add(new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = $"{_faker.Company.CompanyName()} {baseName}",
                        Description = $"Elevate your living space with this {baseName}. Blending contemporary design with unparalleled comfort for your dream home.",
                        Price = _faker.Finance.Amount(rule.MinPrice, rule.MaxPrice),
                        Stock = _faker.Random.Int(5, 100),
                        SubCategoryId = sub.Id,
                        Gender = null,
                        CreatedDate = sub.CreatedDate.AddDays(_faker.Random.Int(1, 90))
                    });
                }
            }
            return products;
        }
    }
}
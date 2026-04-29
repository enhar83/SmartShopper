using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Entity_Layer;
using static Data_Access_Layer.Seeds.ProductSeeds.TechnologySeed;

namespace Data_Access_Layer.Seeds.ProductSeeds
{
    public class ToysAndHobbySeed
    {
        private readonly Faker _faker = new Faker("en");

        private readonly Dictionary<string, SubCategoryRule> _rules = new()
        {
            ["Educational Toys"] = new() { MinPrice = 200, MaxPrice = 4500, MinProducts = 15, MaxProducts = 20, Names = new[] { "STEM Robot Building Kit", "Wooden Solar System Model", "Interactive Language Tablet", "Anatomy Discovery Set" } },
            ["RC Toys"] = new() { MinPrice = 500, MaxPrice = 18000, MinProducts = 5, MaxProducts = 12, Names = new[] { "4K Camera Drone", "High-Speed RC Drift Car", "Remote Control Speedboat", "Stunt Helicopter" } },
            ["Dolls"] = new() { MinPrice = 150, MaxPrice = 6500, MinProducts = 6, MaxProducts = 13, Names = new[] { "Hand-Painted Porcelain Doll", "Fashion Icon Dollhouse Set", "Interactive Baby Doll", "Fairytale Princess Set" } },
            ["Action Figures"] = new() { MinPrice = 100, MaxPrice = 8500, MinProducts = 8, MaxProducts = 12, Names = new[] { "Superpower Hero Series", "Galactic Warrior Figure", "Mythical Creature Set", "Classic Knight Collectible" } },
            ["Models"] = new() { MinPrice = 300, MaxPrice = 12000, MinProducts = 3, MaxProducts = 7, Names = new[] { "WWII Fighter Jet Model", "Luxury Sports Car Assembly", "Historic Sailing Ship Kit", "Architectural Landmark Set" } },
            ["Puzzles"] = new() { MinPrice = 80, MaxPrice = 2500, MinProducts = 3, MaxProducts = 5, Names = new[] { "1000-Piece Landscape Puzzle", "3D Crystal Castle", "Wooden Brain Teaser Box", "Gradient Challenge Puzzle" } },
            ["Board Games"] = new() { MinPrice = 200, MaxPrice = 5500, MinProducts = 2, MaxProducts = 6, Names = new[] { "Kingdom Strategy Game", "Mystery Solving Adventure", "Family Trivia Challenge", "Classic Economic Board Game" } }
        };

        public List<Product> GetProducts(List<SubCategory> allSubCategories)
        {
            var toySubs = allSubCategories.Where(x => x.CategoryId == Guid.Parse("F89F266C-0378-4A17-9D43-115DDECD2835")).ToList();
            var products = new List<Product>();

            foreach (var sub in toySubs)
            {
                if (!_rules.TryGetValue(sub.Name, out var rule)) continue;
                for (int i = 0; i < _faker.Random.Int(rule.MinProducts, rule.MaxProducts); i++)
                {
                    var baseName = _faker.PickRandom(rule.Names);
                    products.Add(new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = $"{_faker.Company.CompanyName()} {baseName}",
                        Description = $"Inspire creativity and fun with this {baseName}. Perfectly safe and engaging for hours of play, designed to spark imagination for all ages.",
                        Price = _faker.Finance.Amount(rule.MinPrice, rule.MaxPrice),
                        Stock = _faker.Random.Int(5, 500),
                        SubCategoryId = sub.Id,
                        Gender = GenderType.Unisex, // Oyuncaklarda genellikle unisex tercih edilir
                        CreatedDate = sub.CreatedDate.AddDays(_faker.Random.Int(1, 90))
                    });
                }
            }
            return products;
        }
    }
}
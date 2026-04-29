using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Entity_Layer;
using static Data_Access_Layer.Seeds.ProductSeeds.TechnologySeed;

namespace Data_Access_Layer.Seeds.ProductSeeds
{
    public class AutomotiveSeed
    {
        private readonly Faker _faker = new Faker("en");

        private readonly Dictionary<string, SubCategoryRule> _rules = new()
        {
            ["Tyres"] = new() { MinPrice = 2000, MaxPrice = 12000, MinProducts = 7, MaxProducts = 15, Names = new[] { "All-Season Performance Tyre", "Winter Grip Series", "Ultra Sport Low Profile", "Rugged Terrain Off-Road" } },
            ["Tools"] = new() { MinPrice = 500, MaxPrice = 15000, MinProducts = 10, MaxProducts = 20, Names = new[] { "Hydraulic Floor Jack", "Digital Torque Wrench", "Professional Socket Set", "OBD2 Diagnostic Scanner" } },
            ["Spare Parts"] = new() { MinPrice = 300, MaxPrice = 25000, MinProducts = 15, MaxProducts = 25, Names = new[] { "Ceramic Brake Pads", "High-Flow Air Filter", "Performance Spark Plug", "Synthetic Oil Filter", "LED Headlight Bulb" } },
            ["Oils"] = new() { MinPrice = 400, MaxPrice = 3500, MinProducts = 6, MaxProducts = 16, Names = new[] { "Full Synthetic 5W-30", "Diesel Engine Armor", "High-Mileage Engine Oil", "Transmission Fluid Pro" } },
            ["Car Care"] = new() { MinPrice = 100, MaxPrice = 2500, MinProducts = 22, MaxProducts = 32, Names = new[] { "Carnauba Car Wax", "Microfiber Wash Mitt", "Interior Detailer Spray", "Ceramic Coating Kit" } }
        };

        public List<Product> GetProducts(List<SubCategory> allSubCategories)
        {
            var autoSubs = allSubCategories.Where(x => x.CategoryId == Guid.Parse("65B20751-CA31-44B9-A407-98FC817B45C2")).ToList();
            var products = new List<Product>();

            foreach (var sub in autoSubs)
            {
                if (!_rules.TryGetValue(sub.Name, out var rule)) continue;
                for (int i = 0; i < _faker.Random.Int(rule.MinProducts, rule.MaxProducts); i++)
                {
                    var baseName = _faker.PickRandom(rule.Names);
                    products.Add(new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = $"{_faker.Company.CompanyName()} {baseName}",
                        Description = $"High-performance {baseName} engineered for safety and durability. Tested under extreme conditions for superior vehicle reliability.",
                        Price = _faker.Finance.Amount(rule.MinPrice, rule.MaxPrice),
                        Stock = _faker.Random.Int(5, 500),
                        SubCategoryId = sub.Id,
                        Gender = null,
                        CreatedDate = sub.CreatedDate.AddDays(_faker.Random.Int(1, 30))
                    });
                }
            }
            return products;
        }
    }
}
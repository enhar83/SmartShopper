using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Entity_Layer;
using static Data_Access_Layer.Seeds.ProductSeeds.TechnologySeed;

namespace Data_Access_Layer.Seeds.ProductSeeds
{
    public class OfficeAndStationerySeed
    {
        private readonly Faker _faker = new Faker("en");

        private readonly Dictionary<string, SubCategoryRule> _rules = new()
        {
            ["Printers"] = new() { MinPrice = 4500, MaxPrice = 45000, MinProducts = 5, MaxProducts = 10, Names = new[] { "LaserJet Pro", "EcoTank High-Yield", "Wireless All-in-One", "OfficeSmart Color Inkjet", "Industrial Label Printer" } },
            ["Notebooks"] = new() { MinPrice = 50, MaxPrice = 1200, MinProducts = 20, MaxProducts = 30, Names = new[] { "Hardcover Journal", "Spiral Meeting Book", "Premium Dotted Notebook", "Leather Executive Planner", "Eco-Friendly Recycled Pad" } },
            ["Furniture"] = new() { MinPrice = 2500, MaxPrice = 35000, MinProducts = 15, MaxProducts = 25, Names = new[] { "Ergonomic Mesh Chair", "L-Shaped Executive Desk", "Electric Standing Desk", "3-Tier Filing Cabinet", "Office Guest Sofa" } },
            ["Pens"] = new() { MinPrice = 10, MaxPrice = 5000, MinProducts = 20, MaxProducts =380, Names = new[] { "Fine Tip Gel Pen", "Luxury Fountain Pen", "Matte Black Rollerball", "Retractable Ballpoint Pack", "Calligraphy Set" } },
            ["Organizers"] = new() { MinPrice = 100, MaxPrice = 2500, MinProducts = 10, MaxProducts = 15, Names = new[] { "Desktop File Sorter", "Acrylic Pen Holder", "Magnetic Whiteboard", "Cable Management Box", "Monitor Stand Riser" } },
            ["Paper"] = new() { MinPrice = 120, MaxPrice = 1500, MinProducts = 3, MaxProducts = 13, Names = new[] { "A4 Copy Paper 80g", "Premium Photo Paper", "Glossy Brochure Sheet", "Recycled Kraft Paper" } }
        };

        public List<Product> GetProducts(List<SubCategory> allSubCategories)
        {
            var officeSubs = allSubCategories.Where(x => x.CategoryId == Guid.Parse("1075B8C4-DE48-4569-90B7-99520C70339A")).ToList();
            var products = new List<Product>();

            foreach (var sub in officeSubs)
            {
                if (!_rules.TryGetValue(sub.Name, out var rule)) continue;
                for (int i = 0; i < _faker.Random.Int(rule.MinProducts, rule.MaxProducts); i++)
                {
                    var baseName = _faker.PickRandom(rule.Names);
                    products.Add(new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = $"{_faker.Company.CompanyName()} {baseName}",
                        Description = $"{baseName} designed for maximum professional efficiency. Built with premium materials for long-term office use.",
                        Price = _faker.Finance.Amount(rule.MinPrice, rule.MaxPrice),
                        Stock = _faker.Random.Int(10, 1000),
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
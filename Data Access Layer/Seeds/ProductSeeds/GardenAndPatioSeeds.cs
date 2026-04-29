using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Entity_Layer;
using static Data_Access_Layer.Seeds.ProductSeeds.TechnologySeed;

namespace Data_Access_Layer.Seeds.ProductSeeds
{
    public class GardenAndPatioSeed
    {
        private readonly Faker _faker = new Faker("en");

        private readonly Dictionary<string, SubCategoryRule> _rules = new()
        {
            ["Garden Tools"] = new()
            {
                MinPrice = 150,
                MaxPrice = 7500,
                MinProducts = 18,
                MaxProducts = 28,
                Names = new[] { "Ergonomic Pruning Shears", "Stainless Steel Garden Trowel", "Heavy Duty Rake", "Long-Handle Digging Shovel", "Electric Hedge Trimmer", "Cordless Leaf Blower" }
            },

            ["Outdoor Furniture"] = new()
            {
                MinPrice = 3500,
                MaxPrice = 65000,
                MinProducts = 13,
                MaxProducts = 23,
                Names = new[] { "Teak Wood Patio Set", "Rattan Outdoor Sofa", "Adjustable Sun Lounger", "Wrought Iron Bistro Table", "All-Weather Dining Set", "Folding Adirondack Chair" }
            },

            ["Lighting"] = new()
            {
                MinPrice = 100,
                MaxPrice = 4500,
                MinProducts = 10,
                MaxProducts = 20,
                Names = new[] { "Solar Path Lights", "LED String Fairy Lights", "Outdoor Wall Lantern", "Motion Sensor Flood Light", "Vintage Post Light", "Floating Pool Lights" }
            },

            ["Pots & Planters"] = new()
            {
                MinPrice = 80,
                MaxPrice = 3500,
                MinProducts = 9,
                MaxProducts = 19,
                Names = new[] { "Self-Watering Planter", "Terracotta Clay Pot", "Ceramic Glazed Flower Pot", "Vertical Garden Wall", "Hanging Basket Planter", "Large Concrete Urn" }
            },

            ["Barbecue"] = new()
            {
                MinPrice = 1200,
                MaxPrice = 45000,
                MinProducts = 7,
                MaxProducts = 17,
                Names = new[] { "Portable Charcoal Grill", "Deluxe Gas BBQ Station", "Wood-Fired Pizza Oven", "Smoker & Grill Combo", "Stainless Steel BBQ Tool Set" }
            },

            ["Plants"] = new()
            {
                MinPrice = 50,
                MaxPrice = 2500,
                MinProducts = 30,
                MaxProducts = 50,
                Names = new[] { "Lavender Bush", "Japanese Maple Sapling", "Dwarf Lemon Tree", "Assorted Succulent Pack", "Potted Snake Plant", "English Ivy Crawler" }
            },

            ["Outdoor Decor"] = new()
            {
                MinPrice = 200,
                MaxPrice = 12000,
                MinProducts = 15,
                MaxProducts = 25,
                Names = new[] { "Zen Garden Fountain", "Wind Chime Harmony", "Decorative Garden Gnome", "Outdoor Canvas Rug", "Metal Wall Art Decor" }
            }
        };

        public List<Product> GetProducts(List<SubCategory> allSubCategories)
        {
            var products = new List<Product>();

            // Garden & Patio Category ID: FB3BA550-08D3-4A74-B1DD-9DC8AB15721D
            var gardenSubs = allSubCategories
                .Where(x => x.CategoryId == Guid.Parse("FB3BA550-08D3-4A74-B1DD-9DC8AB15721D"))
                .ToList();

            foreach (var sub in gardenSubs)
            {
                if (!_rules.TryGetValue(sub.Name, out var rule)) continue;

                int count = _faker.Random.Int(rule.MinProducts, rule.MaxProducts);

                for (int i = 0; i < count; i++)
                {
                    var baseName = _faker.PickRandom(rule.Names);
                    var brand = _faker.Company.CompanyName();

                    string name = $"{brand} {baseName}";
                    string description = GenerateGardenDescription(sub.Name, baseName);

                    products.Add(new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Description = description,
                        Price = _faker.Finance.Amount(rule.MinPrice, rule.MaxPrice),
                        Stock = _faker.Random.Int(5, 150),
                        SubCategoryId = sub.Id,
                        Gender = null, // Bahçe ürünlerinde cinsiyet her zaman null
                        CreatedDate = sub.CreatedDate.AddDays(_faker.Random.Int(1, 90))
                    });
                }
            }
            return products;
        }

        private string GenerateGardenDescription(string subCategory, string productName)
        {
            string[] quality = { "Weather-resistant", "Durable and long-lasting", "Eco-friendly design", "High-performance", "Rust-proof" };
            string[] context = { "designed for outdoor living", "perfect for small balconies or large backyards", "built to withstand harsh conditions", "easy to maintain and clean" };
            string[] appeal = { "adds a touch of elegance to your garden", "makes gardening tasks easier than ever", "creates a cozy outdoor atmosphere", "essential for your patio setup" };

            return $"{_faker.PickRandom(quality)} {productName} that is {_faker.PickRandom(context)}. This item {_faker.PickRandom(appeal)}.";
        }
    }
}
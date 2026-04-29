using Bogus;
using Entity_Layer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data_Access_Layer.Seeds.ProductSeeds
{
    public class TechnologySeed
    {
        private readonly Faker _faker = new Faker("en");

        // Dosya yolu: Data_Access_Layer/Seeds/SubCategoryRule.cs

        public class SubCategoryRule
        {
            public decimal MinPrice { get; set; }
            public decimal MaxPrice { get; set; }
            public int MinProducts { get; set; }
            public int MaxProducts { get; set; }
            public string[] Names { get; set; }
        }


        // Sadece bu sınıfa özel kurallar (Static veya Readonly tutulabilir)
        private readonly Dictionary<string, SubCategoryRule> _rules = new()
        {
            ["Laptops"] = new() { MinPrice = 18000, MaxPrice = 150000, MinProducts = 20, MaxProducts = 30, Names = new[] { "MacBook Pro", "MacBook Air", "ThinkPad X1", "ROG Zephyrus", "Predator Helios", "Legion Pro", "Surface Laptop", "XPS 15", "ZenBook Duo", "EliteBook", "Aspire Nitro", "IdeaPad Slim", "TUF Gaming", "Blade Stealth" } },
            ["Smartphones"] = new() { MinPrice = 9000, MaxPrice = 120000, MinProducts = 15, MaxProducts = 25, Names = new[] { "iPhone", "Galaxy S", "Galaxy A", "Pixel", "Xperia", "Redmi Note", "Mi Ultra", "OnePlus", "Nothing Phone", "Honor Magic", "Realme GT", "Oppo Find X" } },
            ["Tablets"] = new() { MinPrice = 5000, MaxPrice = 50000, MinProducts = 8, MaxProducts = 13, Names = new[] { "iPad Pro", "iPad Air", "Galaxy Tab S", "Galaxy Tab A", "Surface Go", "MatePad", "Lenovo Tab P", "Fire HD Tablet" } },
            ["Audio"] = new() { MinPrice = 700, MaxPrice = 30000, MinProducts = 8, MaxProducts = 15, Names = new[] { "Wireless Headphones", "Bluetooth Earbuds", "Noise Cancelling Headphones", "Studio Headphones", "Gaming Headset", "Soundbar", "Portable Speaker", "Smart Speaker", "Hi-Fi System", "DAC Amp" } },
            ["Gaming"] = new() { MinPrice = 1500, MaxPrice = 40000, MinProducts = 10, MaxProducts = 14, Names = new[] { "Gaming Mouse", "Mechanical Keyboard", "Gaming Headset", "VR Headset", "Gamepad Controller", "Racing Wheel", "Gaming Monitor", "Streaming Kit", "RGB Mousepad" } },
            ["Wearable Tech"] = new() { MinPrice = 2000, MaxPrice = 25000, MinProducts = 5, MaxProducts = 10, Names = new[] { "Smart Watch", "Fitness Tracker", "Smart Band", "Health Monitor Watch", "Sport Watch", "Smart Ring" } },
            ["Cameras"] = new() { MinPrice = 7000, MaxPrice = 80000, MinProducts = 6, MaxProducts = 11, Names = new[] { "DSLR Camera", "Mirrorless Camera", "Action Camera", "Vlog Camera", "Cinema Camera", "Drone Camera" } },
            ["Networking"] = new() { MinPrice = 500, MaxPrice = 15000, MinProducts = 5, MaxProducts = 9, Names = new[] { "WiFi Router", "Mesh WiFi System", "Network Switch", "Access Point", "Fiber Modem", "Range Extender" } },
            ["Accessories"] = new() { MinPrice = 150, MaxPrice = 8000, MinProducts = 10, MaxProducts = 20, Names = new[] { "Power Bank", "Wireless Charger", "USB-C Hub", "Docking Station", "Laptop Stand", "Cooling Pad", "External SSD", "Flash Drive", "Phone Case", "Screen Protector" } }
        };

        public List<Product> GetProducts(List<SubCategory> allSubCategories)
        {
            var products = new List<Product>();

            // Sadece "Technology" kategorisine ait alt kategorileri filtrele
            var technologySubs = allSubCategories
                .Where(x => x.Category?.Name == "Technology")
                .ToList();

            foreach (var sub in technologySubs)
            {
                if (!_rules.TryGetValue(sub.Name, out var rule)) continue;

                int count = _faker.Random.Int(rule.MinProducts, rule.MaxProducts);

                for (int i = 0; i < count; i++)
                {
                    var baseName = _faker.PickRandom(rule.Names);
                    string name = $"{baseName} {_faker.Random.AlphaNumeric(3).ToUpper()} {_faker.Random.Int(1, 99)}";

                    products.Add(new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Description = _faker.Commerce.ProductDescription(),
                        Price = _faker.Finance.Amount(rule.MinPrice, rule.MaxPrice),
                        Stock = GetStockBySubCategory(sub.Name),
                        SubCategoryId = sub.Id,
                        Gender = null,
                        CreatedDate = sub.CreatedDate.AddDays(_faker.Random.Int(1, 30))
                    });
                }
            }

            return products;
        }

        private int GetStockBySubCategory(string subCategoryName)
        {
            return subCategoryName switch
            {
                "Smartphones" => _faker.Random.Int(50, 500),
                "Accessories" => _faker.Random.Int(100, 800),
                "Laptops" => _faker.Random.Int(10, 150),
                "Cameras" => _faker.Random.Int(5, 80),
                _ => _faker.Random.Int(20, 300)
            };
        }
    }
}
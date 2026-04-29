using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Entity_Layer;
using static Data_Access_Layer.Seeds.ProductSeeds.TechnologySeed;

namespace Data_Access_Layer.Seeds.ProductSeeds
{
    public class CosmeticsAndBeautySeed
    {
        private readonly Faker _faker = new Faker("en");

        private readonly Dictionary<string, SubCategoryRule> _rules = new()
        {
            ["Skincare"] = new()
            {
                MinPrice = 150,
                MaxPrice = 2500,
                MinProducts = 12,
                MaxProducts = 20,
                Names = new[] { "Hydrating Serum", "Retinol Night Cream", "Vitamin C Brightening Face Wash", "SPF 50 Sunscreen", "Moisturizing Cream", "Exfoliating Scrub", "Anti-Aging Eye Cream", "Face Oil" }
            },

            ["Hair Care"] = new()
            {
                MinPrice = 100,
                MaxPrice = 1500,
                MinProducts = 15,
                MaxProducts = 27,
                Names = new[] { "Argan Oil Shampoo", "Biotin Conditioner", "Scalp Revitalizing Mask", "Hair Growth Serum", "Dry Shampoo", "Leave-in Conditioner", "Anti-Frizz Oil" }
            },

            ["Makeup"] = new()
            {
                MinPrice = 200,
                MaxPrice = 3500,
                MinProducts = 24,
                MaxProducts = 32,
                Names = new[] { "Matte Lipstick", "Waterproof Mascara", "Long-wear Foundation", "Liquid Eyeliner", "Highlighter Palette", "Blush Duo", "Nude Eyeshadow Palette", "Setting Spray" }
            },

            ["Fragrance"] = new()
            {
                MinPrice = 500,
                MaxPrice = 8000,
                MinProducts = 9,
                MaxProducts = 16,
                Names = new[] { "Eau de Parfum", "Luxury Mist", "Classic Cologne", "Aromatic Essence", "Floral Bouquet Perfume", "Woody Intense Extract" }
            },

            ["Sun Care"] = new()
            {
                MinPrice = 250,
                MaxPrice = 1800,
                MinProducts = 11,
                MaxProducts = 18,
                Names = new[] { "Invisible Sunscreen Stick", "After-sun Cooling Gel", "Tanning Oil", "Ultra Protection Lotion", "Zinc Oxide Mineral Sunscreen" }
            },

            ["Nail Care"] = new()
            {
                MinPrice = 50,
                MaxPrice = 800,
                MinProducts = 8,
                MaxProducts =12,
                Names = new[] { "Gel Nail Polish", "Cuticle Revitalizing Oil", "Strengthening Base Coat", "Top Coat Gloss", "Matte Nail Color", "Nail Polish Remover" }
            },

            ["Personal Care"] = new()
            {
                MinPrice = 40,
                MaxPrice = 600,
                MinProducts = 10,
                MaxProducts = 15,
                Names = new[] { "Organic Body Wash", "Shea Butter Lotion", "Charcoal Toothpaste", "Natural Deodorant Stick", "Hand Sanitizer Mist", "Soft Cotton Buds" }
            }
        };

        public List<Product> GetProducts(List<SubCategory> allSubCategories)
        {
            var products = new List<Product>();

            // CategoryId'si Cosmetics & Beauty olanları filtrele (Senin DB'deki ID: B978F916-1B33-49DE-80DA-C2E84A8BAC92)
            var beautySubs = allSubCategories
                .Where(x => x.CategoryId == Guid.Parse("B978F916-1B33-49DE-80DA-C2E84A8BAC92"))
                .ToList();

            foreach (var sub in beautySubs)
            {
                if (!_rules.TryGetValue(sub.Name, out var rule)) continue;

                int count = _faker.Random.Int(rule.MinProducts, rule.MaxProducts);

                for (int i = 0; i < count; i++)
                {
                    var baseName = _faker.PickRandom(rule.Names);
                    var brand = _faker.Company.CompanyName();
                    var gender = DetermineGender(sub.Name, baseName);

                    string name = $"{brand} {baseName}";
                    string description = GenerateDescription(baseName);

                    products.Add(new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Description = description,
                        Price = _faker.Finance.Amount(rule.MinPrice, rule.MaxPrice),
                        Stock = _faker.Random.Int(50, 1000),
                        SubCategoryId = sub.Id,
                        Gender = gender,
                        CreatedDate = sub.CreatedDate.AddDays(_faker.Random.Int(1, 30))
                    });
                }
            }

            return products;
        }

        private string GenerateDescription(string productName)
        {
            // Bogus'un rastgele kelimeleri yerine anlamlı şablonlar
            string[] adjectives = { "Premium quality", "Enriched with natural ingredients", "Dermatologically tested", "Long-lasting formula", "Luxury feeling" };
            string[] benefits = { "provides a glowy finish", "deeply nourishes your skin", "offers professional results at home", "protects against environmental factors" };

            return $"{_faker.PickRandom(adjectives)} {productName} that {_faker.PickRandom(benefits)}. Perfect for your daily routine.";
        }

        private GenderType? DetermineGender(string subName, string productName)
        {
            string lowerName = productName.ToLower();

            // Fragrance (Parfüm) ve Cologne özelinde cinsiyet ayrımı
            if (lowerName.Contains("cologne") || lowerName.Contains("intense")) return GenderType.Male;
            if (lowerName.Contains("floral") || lowerName.Contains("lipstick") || lowerName.Contains("blush") || lowerName.Contains("eyeshadow")) return GenderType.Female;

            // Kategori bazlı genel tahmin
            return subName switch
            {
                "Makeup" => GenderType.Female,
                "Nail Care" => GenderType.Female,
                _ => GenderType.Unisex // Skincare, Sun Care vb. genelde unisex olur
            };
        }
    }
}
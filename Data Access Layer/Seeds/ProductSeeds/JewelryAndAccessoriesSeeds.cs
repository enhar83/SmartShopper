using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Entity_Layer;
using static Data_Access_Layer.Seeds.ProductSeeds.TechnologySeed;

namespace Data_Access_Layer.Seeds.ProductSeeds
{
    public class JewelryAndAccessoriesSeed
    {
        private readonly Faker _faker = new Faker("en");

        private readonly Dictionary<string, SubCategoryRule> _rules = new()
        {
            ["Watches"] = new()
            {
                MinPrice = 1500,
                MaxPrice = 250000,
                MinProducts = 20,
                MaxProducts = 35,
                Names = new[] { "Chronograph Steel Watch", "Minimalist Leather Timepiece", "Automatic Skeleton Watch", "Luxury Diamond Dial", "Rose Gold Slim Watch", "Sport Digital Series" }
            },

            ["Rings"] = new()
            {
                MinPrice = 800,
                MaxPrice = 50000,
                MinProducts = 15,
                MaxProducts = 25,
                Names = new[] { "Solitaire Diamond Ring", "Sterling Silver Band", "14K Gold Infinity Ring", "Vintage Emerald Ring", "Stackable Minimalist Ring", "Titanium Wedding Band" }
            },

            ["Necklaces"] = new()
            {
                MinPrice = 600,
                MaxPrice = 35000,
                MinProducts = 15,
                MaxProducts = 24,
                Names = new[] { "Pearl Choker", "Gold Plated Pendant", "Silver Chain Necklace", "Crystal Statement Piece", "Custom Nameplate Necklace", "Bohemian Beaded Layer" }
            },

            ["Handbags"] = new()
            {
                MinPrice = 1200,
                MaxPrice = 85000,
                MinProducts = 7,
                MaxProducts = 11,
                Names = new[] { "Leather Crossbody Bag", "Quilted Shoulder Bag", "Luxury Evening Clutch", "Canvas Tote Bag", "Vintage Satchel", "Designer Suede Handbag" }
            },

            ["Sunglasses"] = new()
            {
                MinPrice = 400,
                MaxPrice = 15000,
                MinProducts = 20,
                MaxProducts = 27,
                Names = new[] { "Polarized Aviators", "Classic Wayfarer", "Cat-Eye Fashion Frames", "Retro Round Sunglasses", "Oversized Designer Shades" }
            },

            ["Belts"] = new()
            {
                MinPrice = 300,
                MaxPrice = 7500,
                MinProducts = 8,
                MaxProducts = 18,
                Names = new[] { "Genuine Leather Belt", "Reversible Classic Belt", "Suede Waist Belt", "Slim Dress Belt", "Designer Buckle Belt" }
            },

            ["Wallets"] = new()
            {
                MinPrice = 250,
                MaxPrice = 12000,
                MinProducts = 5,
                MaxProducts = 28,
                Names = new[] { "RFID Blocking Wallet", "Slim Card Holder", "Leather Bi-fold Wallet", "Zip-around Long Wallet", "Crocodile Pattern Purse" }
            },

            ["Hats"] = new()
            {
                MinPrice = 200,
                MaxPrice = 4500,
                MinProducts = 8,
                MaxProducts = 16,
                Names = new[] { "Classic Fedora", "Wool Beret", "Wide Brim Sun Hat", "Cotton Baseball Cap", "Cashmere Beanie" }
            }
        };

        public List<Product> GetProducts(List<SubCategory> allSubCategories)
        {
            var products = new List<Product>();

            // Jewelry & Accessories Category ID: 48362AF3-4704-4831-BDD8-A9EA3B653594
            var jewelrySubs = allSubCategories
                .Where(x => x.CategoryId == Guid.Parse("48362AF3-4704-4831-BDD8-A9EA3B653594"))
                .ToList();

            foreach (var sub in jewelrySubs)
            {
                if (!_rules.TryGetValue(sub.Name, out var rule)) continue;

                int count = _faker.Random.Int(rule.MinProducts, rule.MaxProducts);

                for (int i = 0; i < count; i++)
                {
                    var baseName = _faker.PickRandom(rule.Names);
                    var brand = _faker.Company.CompanyName();
                    var gender = DetermineGender(sub.Name, baseName);

                    string name = $"{brand} {baseName}";
                    string description = GenerateJewelryDescription(sub.Name, baseName);

                    products.Add(new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Description = description,
                        Price = _faker.Finance.Amount(rule.MinPrice, rule.MaxPrice),
                        Stock = _faker.Random.Int(10, 200),
                        SubCategoryId = sub.Id,
                        Gender = gender,
                        CreatedDate = sub.CreatedDate.AddDays(_faker.Random.Int(1, 45))
                    });
                }
            }
            return products;
        }

        private string GenerateJewelryDescription(string subCategory, string productName)
        {
            string[] craftsmanship = { "Exquisitely crafted", "Masterfully designed", "Elegant and timeless", "A perfect blend of style and comfort", "Hand-finished detail" };
            string[] materials = { "premium materials", "sustainable sources", "high-grade finishes", "attention to every detail" };
            string[] occasion = { "ideal for special occasions", "perfect for daily elegance", "a stunning gift for loved ones", "elevates any outfit instantly" };

            return $"{_faker.PickRandom(craftsmanship)} {productName}. Made with {_faker.PickRandom(materials)}, this piece is {_faker.PickRandom(occasion)}.";
        }

        private GenderType? DetermineGender(string subName, string productName)
        {
            string lowerName = productName.ToLower();

            // Çantalar ve bazı takılar için kadın odaklı varsayım
            if (subName == "Handbags" || lowerName.Contains("clutch") || lowerName.Contains("purse") || lowerName.Contains("choker")) return GenderType.Female;

            // Düğün bandı veya klasik kemer gibi ürünler için unisex/erkek tahmini
            if (lowerName.Contains("wedding band") || lowerName.Contains("cologne") || lowerName.Contains("bi-fold")) return GenderType.Unisex;

            // Kategori bazlı genel tahmin
            return subName switch
            {
                "Watches" => _faker.PickRandom(GenderType.Male, GenderType.Female, GenderType.Unisex),
                "Sunglasses" => GenderType.Unisex,
                "Rings" => GenderType.Female,
                "Necklaces" => GenderType.Female,
                _ => GenderType.Unisex
            };
        }
    }
}
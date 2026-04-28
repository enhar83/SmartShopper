using Bogus;
using Entity_Layer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data_Access_Layer.Seeds
{
    public class SeedService
    {
        // Senior Notu: Diğer metodlardan erişim için listeleri property olarak tutuyoruz.
        public List<Category> GeneratedCategories { get; private set; } = new List<Category>();
        public List<SubCategory> GeneratedSubCategories { get; private set; } = new List<SubCategory>();

        public void SeedCategoriesAndSubCategories()
        {
            var faker = new Faker("en");

            // Veri üretiminde tutarlılık (her çalıştırmada aynı ID'ler) için sabit seed.
            Bogus.Randomizer.Seed = new Random(42);

            // 1. KATEGORİ TANIMLARI
            var categoryDefinitions = new List<(string Name, bool HasGender, string Desc)>
            {
                ("Technology", false, "State-of-the-art electronics and computing gadgets."),
                ("Fashion & Apparel", true, "Modern clothing and seasonal fashion collections."),
                ("Home & Furniture", false, "Stylish furniture and home decoration items."),
                ("Cosmetics & Beauty", true, "Premium beauty, skincare, and personal care."),
                ("Sports & Outdoor", true, "Equipment for athletes and outdoor adventurers."),
                ("Footwear", true, "Shoes, sneakers, and boots for all lifestyles."),
                ("Books & Media", false, "Wide range of literature and educational materials."),
                ("Automotive", false, "High-performance car parts and vehicle accessories."),
                ("Baby & Kids", true, "Essentials, clothing, and toys for infants and children."),
                ("Jewelry & Accessories", true, "Exquisite jewelry, watches, and fashion accessories."),
                ("Supermarket", false, "Fresh groceries, beverages, and household goods."),
                ("Pet Supplies", false, "Everything your pets need for a happy life."),
                ("Office & Stationery", false, "Professional office supplies and stationery."),
                ("Toys & Hobby", true, "Creative toys, puzzles, and hobby kits."),
                ("Garden & Patio", false, "Gardening tools and outdoor living equipment.")
            };

            // Kategorileri Oluştur (3 yıl öncesine set edildi)
            GeneratedCategories = categoryDefinitions.Select(x => new Category
            {
                Id = Guid.NewGuid(),
                Name = x.Name,
                HasGender = x.HasGender,
                Description = x.Desc,
                CreatedDate = DateTime.Now.AddYears(-3), // Başlangıç 3 yıl önce
                IsDeleted = false
            }).ToList();

            // 2. ALT KATEGORİ HARİTASI
            var subCategoryMap = new Dictionary<string, string[]>
            {
                { "Technology", new[] { "Laptops", "Smartphones", "Audio", "Cameras", "Gaming", "Accessories", "Tablets", "Wearable Tech", "Networking" } },
                { "Fashion & Apparel", new[] { "T-Shirts", "Jeans", "Dresses", "Jackets", "Activewear", "Suits", "Knitwear", "Sleepwear", "Swimwear" } },
                { "Home & Furniture", new[] { "Kitchen", "Living Room", "Bedroom", "Decoration", "Office Furniture", "Lighting", "Storage", "Bathroom" } },
                { "Cosmetics & Beauty", new[] { "Skincare", "Makeup", "Fragrance", "Hair Care", "Personal Care", "Sun Care", "Nail Care" } },
                { "Sports & Outdoor", new[] { "Fitness", "Camping", "Cycling", "Winter Sports", "Water Sports", "Team Sports", "Hiking" } },
                { "Footwear", new[] { "Sneakers", "Boots", "Casual", "Formal", "Sport Shoes", "Slippers", "Sandals" } },
                { "Books & Media", new[] { "Fiction", "Non-Fiction", "Self-Help", "Educational", "Children Books", "Sci-Fi", "Comics", "Biographies" } },
                { "Automotive", new[] { "Spare Parts", "Tyres", "Oils", "Accessories", "Electronics", "Car Care", "Tools" } },
                { "Baby & Kids", new[] { "Clothing", "Toys", "Nursing", "Strollers", "Feeding", "Nursery", "Diapering", "Safety" } },
                { "Jewelry & Accessories", new[] { "Watches", "Necklaces", "Sunglasses", "Wallets", "Handbags", "Rings", "Belts", "Hats" } },
                { "Supermarket", new[] { "Beverages", "Snacks", "Canned Food", "Cleaning", "Personal Care", "Frozen Food", "Bakery", "Dairy" } },
                { "Pet Supplies", new[] { "Dog Food", "Cat Food", "Toys", "Health", "Grooming", "Aquarium", "Small Pet Care" } },
                { "Office & Stationery", new[] { "Pens", "Notebooks", "Organizers", "Paper", "Furniture", "Printers", "Desk Supplies" } },
                { "Toys & Hobby", new[] { "Action Figures", "Puzzles", "Board Games", "RC Toys", "Models", "Educational Toys", "Dolls" } },
                { "Garden & Patio", new[] { "Garden Tools", "Outdoor Furniture", "Plants", "Barbecue", "Lighting", "Pots & Planters", "Outdoor Decor" } }
            };

            // 3. EŞLEŞTİRME VE ÜRETİM
            foreach (var category in GeneratedCategories)
            {
                // CS8600 Uyarısını engellemek için string[]? (nullable) kullanımı
                if (subCategoryMap.TryGetValue(category.Name, out string[]? subNames) && subNames != null)
                {
                    foreach (var subName in subNames)
                    {
                        GeneratedSubCategories.Add(new SubCategory
                        {
                            Id = Guid.NewGuid(),
                            Name = subName,
                            CategoryId = category.Id,
                            Category = category, // Null-Safety için navigation property ataması
                            // Alt kategoriler ana kategoriden 1-30 gün sonra oluşturulmuş gibi simüle ediliyor
                            CreatedDate = category.CreatedDate.AddDays(faker.Random.Int(1, 30))
                        });
                    }
                }
            }
        }
    }
}
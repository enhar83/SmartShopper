using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Entity_Layer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data_Access_Layer.DbContext
{
    public class AppDbContext: IdentityDbContext<AppUser, AppRole, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<CustomerSegmentationResult> CustomerSegmentationResults { get; set; }
        public DbSet<CustomerChurnResult> CustomerChurnResults { get; set; }
        public DbSet<ProductSalesForecast> ProductSalesForecasts { get; set; }
        public DbSet<OrderAnomalyResult> OrderAnomalyResults { get; set; }
        public DbSet<SubCategoryDemandForecast> SubCategoryDemandForecasts { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<DiscountCustomer> DiscountCustomers { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentAnalysisResult> CommentAnalysisResults { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region Identity & AppUser Configuration
            builder.Entity<AppUser>(entity =>
            {
                entity.ToTable("Users");
                entity.Property(u => u.Name).HasMaxLength(50).IsRequired();
                entity.Property(u => u.Surname).HasMaxLength(50).IsRequired();
                entity.Property(u => u.ImageUrl).HasMaxLength(500);

                entity.HasMany(u => u.Orders)
                      .WithOne(o => o.AppUser)
                      .HasForeignKey(o => o.AppUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<AppRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
            #endregion

            #region Product & Category Configuration
            builder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Name).HasMaxLength(150).IsRequired();
                entity.Property(p => p.Description).HasMaxLength(1000).IsRequired();
                entity.Property(p => p.Price).HasPrecision(18, 2);
                entity.HasIndex(p => p.Name);

                entity.HasOne(p => p.SubCategory)
                      .WithMany(sc => sc.Products)
                      .HasForeignKey(p => p.SubCategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            #endregion

            #region Order & OrderItem Configuration
            builder.Entity<Order>(entity =>
            {
                entity.Property(o => o.TotalPrice).HasPrecision(18, 2);
                entity.Property(o => o.DeliveryAddressSnapshot).HasMaxLength(1000).IsRequired();
                entity.HasOne(o => o.UserAddress)
                      .WithMany(a => a.Orders)
                      .HasForeignKey(o => o.AddressId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<OrderItem>(entity =>
            {
                entity.Property(oi => oi.PriceAtPurchase).HasPrecision(18, 2);

                entity.HasOne(oi => oi.Product)
                      .WithMany(p => p.OrderItems)
                      .HasForeignKey(oi => oi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            #endregion

            #region Other Entities (Images, Address, Favorites)
            builder.Entity<UserAddress>(entity =>
            {
                entity.Property(a => a.Title).HasMaxLength(50).IsRequired();
                entity.Property(a => a.Country).HasMaxLength(50).IsRequired();
                entity.Property(a => a.City).HasMaxLength(50).IsRequired();
                entity.Property(a => a.District).HasMaxLength(50);
                entity.Property(a => a.FullAddress).HasMaxLength(500).IsRequired();

                entity.HasOne(a => a.AppUser)
                      .WithMany(u => u.Addresses)
                      .HasForeignKey(a => a.AppUserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Favorite>(entity =>
            {
                entity.HasIndex(f => new { f.AppUserId, f.ProductId }).IsUnique();
            });
            #endregion

            #region CustomerSegmentation
            builder.Entity<CustomerSegmentationResult>(entity =>
            {
                entity.Property(e => e.SegmentLabel)
                .IsRequired()
                .HasMaxLength(50);

                entity.Property(e => e.ConfidenceScore)
                    .HasDefaultValue(0.0);

                entity.HasOne(d => d.AppUser)
                    .WithMany(p => p.SegmentationResults)
                    .HasForeignKey(d => d.AppUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.AppUserId);
            });
            #endregion

            #region CustomerChurnResult
            builder.Entity<CustomerChurnResult>(entity =>
            {
                entity.ToTable("CustomerChurnResults");

                entity.HasOne(x => x.AppUser)
                    .WithOne(u => u.CustomerChurnResult)
                    .HasForeignKey<CustomerChurnResult>(x => x.AppUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.ChurnProbability)
                    .HasPrecision(18, 4) 
                    .IsRequired();

                entity.Property(e => e.Monetary)
                    .HasPrecision(18, 2) 
                    .IsRequired();

                entity.HasIndex(e => e.AppUserId).IsUnique();
                entity.HasIndex(e => e.IsChurn); 
            });
            #endregion

            #region ProductSalesForecast Configuration
            builder.Entity<ProductSalesForecast>(entity =>
            {
                entity.ToTable("ProductSalesForecasts");

                entity.HasOne(e => e.Product)
                      .WithMany() 
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(f => f.Product)     
                      .WithMany(p => p.SalesForecasts)   
                      .HasForeignKey(f => f.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.ExpectedRevenue)
                      .HasPrecision(18, 2)
                      .IsRequired();

                entity.Property(e => e.ConfidenceScore)
                      .HasDefaultValue(0.0);

                entity.HasIndex(e => new { e.ProductId, e.TargetYear, e.TargetMonth })
                      .IsUnique()
                      .HasDatabaseName("IX_ProductForecast_Product_Date");
            });
            #endregion

            #region OrderAnomalyResult Configuration
            builder.Entity<OrderAnomalyResult>(entity =>
            {
                entity.ToTable("OrderAnomalyResults");

                entity.HasOne(x => x.Order)
                      .WithOne(o => o.OrderAnomalyResult) 
                      .HasForeignKey<OrderAnomalyResult>(x => x.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.Score).IsRequired();
                entity.Property(e => e.PValue).IsRequired();

                entity.HasIndex(e => e.IsAnomaly);

                entity.HasIndex(e => e.OrderId).IsUnique();
            });
            #endregion

            #region SubCategoryDemandForecast Configuration
            builder.Entity<SubCategoryDemandForecast>(entity =>
            {
                entity.ToTable("SubCategoryDemandForecasts");

                entity.HasOne(e => e.SubCategory)
                      .WithMany(s => s.DemandForecasts) 
                      .HasForeignKey(e => e.SubCategoryId)
                      .OnDelete(DeleteBehavior.Cascade); 

                entity.Property(e => e.PredictedRevenue)
                      .HasPrecision(18, 2)
                      .IsRequired();

                entity.Property(e => e.ModelAccuracyScore)
                      .HasDefaultValue(0.0);

                entity.HasIndex(e => new { e.SubCategoryId, e.TargetYear, e.TargetMonth })
                      .IsUnique()
                      .HasDatabaseName("IX_SubCatForecast_SubCat_Date");
            });
            #endregion

            #region Discount & DiscountCustomer Configuration
            builder.Entity<Discount>(entity =>
            {
                entity.ToTable("Discounts");

                entity.Property(d => d.Name)
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(d => d.Description)
                      .HasMaxLength(500);

                entity.Property(d => d.Value)
                      .HasPrecision(18, 2)
                      .IsRequired();

                entity.Property(d => d.MinOrderAmount)
                      .HasPrecision(18, 2);

                entity.Property(d => d.MaxDiscountAmount)
                      .HasPrecision(18, 2);
            });

            builder.Entity<DiscountCustomer>(entity =>
            {
                entity.ToTable("DiscountCustomers");

                entity.HasOne(dc => dc.AppUser)
                      .WithMany(u => u.DiscountCustomers)
                      .HasForeignKey(dc => dc.AppUserId)
                      .OnDelete(DeleteBehavior.Cascade); 

                entity.HasOne(dc => dc.Discount)
                      .WithMany(d => d.DiscountCustomers)
                      .HasForeignKey(dc => dc.DiscountId)
                      .OnDelete(DeleteBehavior.Cascade); 

                entity.HasIndex(dc => new { dc.AppUserId, dc.IsUsed });
            });
            #endregion

            #region Comment Configuration
            builder.Entity<Comment>(entity =>
            {
                entity.ToTable("Comments");

                entity.Property(c => c.Title)
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(c => c.Text)
                      .HasMaxLength(1000)
                      .IsRequired();

                entity.Property(c => c.IsApproved)
                      .HasDefaultValue(false);

                // Product ile İlişki (Ürün silinirse yorumlar da silinsin)
                entity.HasOne(c => c.Product)
                      .WithMany(p => p.Comments)
                      .HasForeignKey(c => c.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                // AppUser ile İlişki (Kullanıcı silinirse yorumlar kalsın, db patlamasın)
                entity.HasOne(c => c.AppUser)
                      .WithMany(u => u.Comments)
                      .HasForeignKey(c => c.AppUserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });
            #endregion

            #region CommentAnalysisResult Configuration
            builder.Entity<CommentAnalysisResult>(entity =>
            {
                entity.ToTable("CommentAnalysisResults");

                entity.HasOne(x => x.Comment)
                      .WithOne(c => c.CommentAnalysisResult)
                      .HasForeignKey<CommentAnalysisResult>(x => x.CommentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.ToxicityScore)
                      .HasPrecision(18, 4)
                      .IsRequired();

                entity.Property(e => e.SentimentScore)
                      .HasPrecision(18, 4)
                      .HasDefaultValue(0.0);

                entity.HasIndex(e => e.CommentId).IsUnique(); 
                entity.HasIndex(e => e.IsToxic); 
            });
            #endregion

            #region Notification Configuration
            builder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");

                entity.Property(n => n.Title)
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(n => n.Message)
                      .HasMaxLength(500)
                      .IsRequired();

                entity.Property(n => n.NotificationType)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(n => n.RelatedUrl)
                      .HasMaxLength(500);

                entity.Property(n => n.IsRead)
                      .HasDefaultValue(false);

                entity.HasOne(n => n.AppUser)
                      .WithMany(u => u.Notifications) 
                      .HasForeignKey(n => n.AppUserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            #endregion
        }
    }
}

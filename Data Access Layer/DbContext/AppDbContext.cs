using System;
using System.Collections.Generic;
using System.Linq;
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
        }
    }
}

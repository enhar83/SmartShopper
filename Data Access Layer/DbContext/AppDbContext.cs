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

            //appuser 
            builder.Entity<AppUser>(entity =>
            {
                entity.Property(u => u.Name).HasMaxLength(50).IsRequired();
                entity.Property(u => u.Surname).HasMaxLength(50).IsRequired();
                entity.Property(u => u.ImageUrl).HasMaxLength(500);

            });

            //product
            builder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Name).HasMaxLength(150).IsRequired();
                entity.Property(p => p.Description).HasMaxLength(1000).IsRequired();

                entity.Property(p => p.Price).HasPrecision(18, 2); //virgülden sonra iki basamak getirmesini sağlar.

            });

            //category
            builder.Entity<Category>(entity =>
            {
                entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
                entity.Property(c => c.Description).HasMaxLength(500);
            });

            builder.Entity<SubCategory>(entity =>
            {
                entity.Property(sc => sc.Name).HasMaxLength(100).IsRequired();
            });

            //order ve orderitem
            builder.Entity<OrderItem>(entity =>
            {
                entity.Property(oi => oi.PriceAtPurchase).HasPrecision(18, 2);
            });

            //product image
            builder.Entity<ProductImage>(entity =>
            {
                entity.Property(pi => pi.ImageUrl).HasMaxLength(500).IsRequired();
            });

            //useradress
            builder.Entity<UserAddress>(entity =>
            {
                entity.Property(a => a.Title).HasMaxLength(50).IsRequired();
                entity.Property(a => a.Country).HasMaxLength(50).IsRequired();
                entity.Property(a => a.City).HasMaxLength(50).IsRequired();
                entity.Property(a => a.District).HasMaxLength(50);
                entity.Property(a => a.FullAddress).HasMaxLength(500).IsRequired();
            });

            //
            //varsayılan identity tablolarının isimlerini daha temiz hale getirir.
            builder.Entity<AppUser>().ToTable("Users");
            builder.Entity<AppRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        }
    }
}

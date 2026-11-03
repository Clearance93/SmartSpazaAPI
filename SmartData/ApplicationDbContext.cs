using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartModels;

namespace SmartData
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ApplicationUser> User { get; set; }

        public DbSet<Availability> Availability { get; set; }

        public DbSet<BusinessNotificationPreference> BusinessNotificationPreference { get; set; }

        public DbSet<BusinessProfile> BusinessProfile { get; set; }

        public DbSet<BusinessTeamMembers> BusinessTeamMembers { get; set; }

        public DbSet<Delivery> Deliveries { get; set; }

        public DbSet<DeliveryConfiguration> DeliveryConfiguration { get; set; }

        public DbSet<DeliveryItems> DeliveryItems { get; set; }

        public DbSet<DeliveryRatings> DeliveryRatings { get; set; }

        public DbSet<DeliveryTracking> DeliveryTracking { get; set; }

        public DbSet<Drivers> Drivers { get; set; }

        public DbSet<DriversAvailability> DriversAvailabilities { get; set; }

        public DbSet<Navigation> Navigation { get; set; }

        public DbSet<OperatingHours> OperatingHours { get; set; }

        public DbSet<SupplierBusinessInfo> SupplierBusinessInfo { get; set; }

        public DbSet<SupplierBusinessNotification> SupplierBusinessNotifications { get; set; }

        public DbSet<SystemNotification> SystemNotification { get; set; }

        public DbSet<Vehicle> Vehicles { get; set; }

        public DbSet<ShopsOwner> ShopsOwners { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<DeliveryItems>().HasQueryFilter(item => !item.IsDeleted);
            builder.Entity<Product>().HasQueryFilter(product => !product.IsDeleted);

            builder.Entity<DeliveryItems>().HasIndex(item => item.DeliveryId);
            builder.Entity<DeliveryTracking>().HasIndex(tracking => tracking.DriverId);
            builder.Entity<DeliveryRatings>().HasIndex(rating => rating.DriverId);
            builder.Entity<Navigation>().HasIndex(navigation => navigation.DriverId);
            builder.Entity<Vehicle>().HasIndex(vehicle => vehicle.DriverId);
            builder.Entity<Availability>().HasIndex(availability => availability.Uid);
            builder.Entity<Product>().HasIndex(product => new { product.Barcode, product.ShopOwnerId })
                                      .IsUnique()
                                      .HasFilter("[Barcode] IS NOT NULL AND [ShopOwnerId] IS NOT NULL AND [IsDeleted] = 0");

            builder.Entity<Product>().HasIndex(product => new { product.Barcode, product.SupplierId })
                                      .IsUnique()
                                      .HasFilter("[Barcode] IS NOT NULL AND [SupplierId] IS NOT NULL AND [IsDeleted] = 0");

            builder.Entity<RefreshToken>(entity =>
            {
                entity.Property(token => token.TokenHash).HasMaxLength(64).IsRequired();
                entity.Property(token => token.UserId).HasMaxLength(450).IsRequired();
                entity.HasIndex(token => token.TokenHash).IsUnique();
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(token => token.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

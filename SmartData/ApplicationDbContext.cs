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

        public DbSet<DeliveryPreference> DeliveryPreference { get; set; }

        public DbSet<DeliveryRecord> DeliveryRecord { get; set; }

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
    }
}

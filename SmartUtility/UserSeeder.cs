using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartData;
using SmartEnums;
using SmartModels;

namespace SmartUtility
{
    public class UserSeeder
    {
        public const string DemoEmail = "demo@smartspaza.co.za";
        public const string SupplierDemoEmail = "supplier@smartspaza.co.za";
        public const string DriverDemoEmail = "driver@smartspaza.co.za";
        public const string DemoPassword = "Spaza_123";

        private static readonly (string Email, string FullName, string WhatsApp, string Role)[] DemoUsers =
        {
            (DemoEmail, "Demo Spaza Owner", "0710000000", RoleNames.SpazaOwner),
            (SupplierDemoEmail, "Demo Supplier", "0720000000", RoleNames.Supplier),
            (DriverDemoEmail, "Demo Driver", "0730000000", RoleNames.Driver),
        };

        public static async Task SeedDemoUserAsync(UserManager<ApplicationUser> userManager,
                                                   RoleManager<IdentityRole> roleManager,
                                                   ApplicationDbContext context,
                                                   ILogger? logger = null)
        {
            foreach (var demo in DemoUsers)
            {
                var user = await userManager.FindByEmailAsync(demo.Email);

                if (user is null)
                {
                    user = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = demo.Email,
                        Email = demo.Email,
                        EmailConfirmed = true,
                        FullName = demo.FullName,
                        WhatsAppNumber = demo.WhatsApp,
                        CreationTime = DateTime.UtcNow
                    };

                    var created = await userManager.CreateAsync(user, DemoPassword);

                    if (!created.Succeeded)
                    {
                        logger?.LogError("Failed to create demo user '{Email}': {Errors}",
                            demo.Email,
                            string.Join(", ", created.Errors.Select(e => e.Description)));
                        continue;
                    }

                    if (!await roleManager.RoleExistsAsync(demo.Role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(demo.Role));
                    }

                    await userManager.AddToRoleAsync(user, demo.Role);

                    logger?.LogInformation("Seeded demo user '{Email}' with role '{Role}'.", demo.Email, demo.Role);
                }

                await EnsureDemoProfileAsync(context, user, demo.Role, logger);
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureDemoProfileAsync(ApplicationDbContext context,
                                                         ApplicationUser user,
                                                         string role,
                                                         ILogger? logger)
        {
            if (!Guid.TryParse(user.Id, out var userId))
            {
                return;
            }

            switch (role)
            {
                case RoleNames.Supplier:
                    if (!await context.Suppliers.AnyAsync(supplier => supplier.SupplierId == userId))
                    {
                        context.Suppliers.Add(new Supplier
                        {
                            SupplierId = userId,
                            SupplierName = "Demo Supplier",
                            ContactName = user.FullName,
                            Email = user.Email,
                            ContactNumber = user.WhatsAppNumber,
                            CreatedAt = DateTime.UtcNow
                        });

                        logger?.LogInformation("Seeded supplier profile for '{Email}'.", user.Email);
                    }

                    break;

                case RoleNames.SpazaOwner:
                    if (!await context.ShopsOwners.AnyAsync(shop => shop.ShopOwnerId == userId))
                    {
                        context.ShopsOwners.Add(new ShopsOwner
                        {
                            ShopOwnerId = userId,
                            NameOfTheShops = "Demo Spaza Shop",
                            OwnerName = user.FullName,
                            PhoneNumber = user.WhatsAppNumber,
                            Email = user.Email,
                            ShopType = ShopType.Spaza,
                            CreatedAt = DateTime.UtcNow
                        });

                        logger?.LogInformation("Seeded shop profile for '{Email}'.", user.Email);
                    }

                    break;

                case RoleNames.Driver:
                    if (!await context.Drivers.AnyAsync(driver => driver.DriverId == userId))
                    {
                        context.Drivers.Add(new Drivers
                        {
                            DriverId = userId,
                            FullName = user.FullName,
                            Email = user.Email,
                            PhoneNumbers = user.WhatsAppNumber,
                            WhatsAppNumber = user.WhatsAppNumber,
                            CreationTime = DateTime.UtcNow
                        });

                        logger?.LogInformation("Seeded driver profile for '{Email}'.", user.Email);
                    }

                    if (!await context.Availability.AnyAsync(availability => availability.Uid == userId))
                    {
                        context.Availability.Add(new Availability
                        {
                            AvailabilityId = Guid.NewGuid(),
                            Uid = userId,
                            CreationTime = DateTime.UtcNow
                        });
                    }

                    if (!await context.Navigation.AnyAsync(navigation => navigation.DriverId == userId))
                    {
                        context.Navigation.Add(new Navigation
                        {
                            NavigationId = Guid.NewGuid(),
                            DriverId = userId,
                            CreationTime = DateTime.UtcNow
                        });
                    }

                    if (!await context.SystemNotification.AnyAsync(notification => notification.Uid == userId))
                    {
                        context.SystemNotification.Add(new SystemNotification
                        {
                            SystemNotificationId = Guid.NewGuid(),
                            Uid = userId,
                            NewAssignments = true,
                            CustomerMessages = true,
                            SystemUpdate = true,
                            CreationTime = DateTime.UtcNow
                        });
                    }

                    break;
            }
        }
    }
}

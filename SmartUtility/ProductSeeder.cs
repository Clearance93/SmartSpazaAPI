using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartData;
using SmartModels;

namespace SmartUtility
{
    public class ProductSeeder
    {
        public static async Task SeedProductsAsync(ApplicationDbContext context, ILogger? logger = null)
        {
            await EnsureValidEan13BarcodesAsync(context, logger);

            if (await context.Products.AnyAsync())
            {
                logger?.LogInformation("Products already exist, skipping product seeding.");
                return;
            }

            var supplierId = await GetDemoSupplierIdAsync(context);
            var products = new List<Product>
            {
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    Barcode = "6009510800128",
                    StockKeepUnit = "SKU-BREAD-WHITE",
                    Name = "White Bread Loaf",
                    Category = "Bakery",
                    Price = 18.99m,
                    StockQuantity = 40,
                    Unit = "loaf",
                    SupplierId = supplierId,
                    IsDeleted = false,
                    CreationTime = DateTime.UtcNow
                },
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    Barcode = "6001234500018",
                    StockKeepUnit = "SKU-MILK-2L",
                    Name = "Full Cream Milk 2L",
                    Category = "Dairy",
                    Price = 34.50m,
                    StockQuantity = 25,
                    Unit = "bottle",
                    IsDeleted = false,
                    SupplierId = supplierId,
                    CreationTime = DateTime.UtcNow
                },
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    Barcode = "6009880222339",
                    StockKeepUnit = "SKU-MAIZE-1KG",
                    Name = "Maize Meal 1kg",
                    Category = "Staples",
                    Price = 22.00m,
                    StockQuantity = 60,
                    Unit = "pack",
                    IsDeleted = false,
                    SupplierId = supplierId,
                    CreationTime = DateTime.UtcNow
                },
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    Barcode = "6001100203043",
                    StockKeepUnit = "SKU-SUGAR-2KG",
                    Name = "White Sugar 2kg",
                    Category = "Staples",
                    Price = 45.99m,
                    StockQuantity = 30,
                    Unit = "pack",
                    IsDeleted = false,
                    SupplierId = supplierId,
                    CreationTime = DateTime.UtcNow
                },
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    Barcode = "5449000000996",
                    StockKeepUnit = "SKU-COKE-500ML",
                    Name = "Coca-Cola 500ml",
                    Category = "Beverages",
                    Price = 12.50m,
                    StockQuantity = 96,
                    Unit = "bottle",
                    IsDeleted = false,
                    SupplierId = supplierId,
                    CreationTime = DateTime.UtcNow
                },
                new Product
                {
                    ProductId = Guid.NewGuid(),
                    Barcode = "6002213700085",
                    StockKeepUnit = "SKU-EGGS-6",
                    Name = "Eggs (Half Dozen)",
                    Category = "Dairy",
                    Price = 19.99m,
                    StockQuantity = 48,
                    Unit = "tray",
                    IsDeleted = false,
                    SupplierId = supplierId,
                    CreationTime = DateTime.UtcNow
                }
            };

            await context.Products.AddRangeAsync(products);

            await context.SaveChangesAsync();

            logger?.LogInformation($"Seeded {products.Count} sample products successfully.");
        }

        /// <summary>
        /// Retail barcodes must be valid EAN-13 GTINs, otherwise an EAN-13 barcode
        /// cannot be drawn or scanned. Repairs any 13-digit numeric barcode whose
        /// check digit is wrong (keeps the first 12 digits, fixes the check digit).
        /// </summary>
        private static async Task EnsureValidEan13BarcodesAsync(ApplicationDbContext context, ILogger? logger)        {
            var products = await context.Products
                .Where(product => product.Barcode != null)
                .ToListAsync();

            var repaired = 0;

            foreach (var product in products)
            {
                var barcode = product.Barcode?.Trim();

                if (barcode is null ||
                    barcode.Length != 13 ||
                    !barcode.All(char.IsAsciiDigit) ||
                    IsValidEan13(barcode))
                {
                    continue;
                }

                var corrected = WithEan13CheckDigit(barcode);

                var clash = await context.Products
                    .AnyAsync(other => other.Barcode == corrected && other.ProductId != product.ProductId);

                if (clash)
                {
                    logger?.LogWarning("Cannot repair barcode {Barcode} on {Product}: {Corrected} already exists.",
                                       barcode, product.Name, corrected);
                    continue;
                }

                product.Barcode = corrected;
                product.Updated = DateTime.UtcNow;
                repaired++;

                logger?.LogInformation("Repaired barcode for {Product}: {Old} -> {New}.",
                                       product.Name, barcode, corrected);
            }

            if (repaired > 0)
            {
                await context.SaveChangesAsync();
                logger?.LogInformation("Repaired {Count} invalid EAN-13 barcode(s).", repaired);
            }
        }

        private static async Task<Guid?> GetDemoSupplierIdAsync(ApplicationDbContext context)
        {
            var supplierUser = await context.Users
                .FirstOrDefaultAsync(user => user.Email == UserSeeder.SupplierDemoEmail);

            return supplierUser != null && Guid.TryParse(supplierUser.Id, out var supplierId)
                ? supplierId
                : null;
        }

        private static bool IsValidEan13(string barcode)
        {
            if (barcode.Length != 13 || !barcode.All(char.IsAsciiDigit))
            {
                return false;
            }

            var sum = 0;

            for (var i = 0; i < 12; i++)
            {
                sum += (barcode[i] - '0') * (i % 2 == 0 ? 1 : 3);
            }

            return (10 - (sum % 10)) % 10 == barcode[12] - '0';
        }

        private static string WithEan13CheckDigit(string barcode)
        {
            var body = barcode[..12];
            var sum = 0;

            for (var i = 0; i < 12; i++)
            {
                sum += (body[i] - '0') * (i % 2 == 0 ? 1 : 3);
            }

            var checkDigit = (10 - (sum % 10)) % 10;

            return body + checkDigit;
        }
    }
}

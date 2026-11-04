using Microsoft.EntityFrameworkCore;
using SmartData;
using SmartInterfaces.IRepository.IInventory;
using SmartModels;

namespace SmartRepository.Inventory
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _Context;

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Looks up a barcode only inside the caller's own catalogue, so one business
        /// can never see or sell another business's product.
        /// </summary>
        public async Task<Product?> GetByBarcodeAsync(string barcode, Guid ownerId)
        {
            return await _Context.Products
                .FirstOrDefaultAsync(product =>
                    product.Barcode == barcode &&
                    !product.IsDeleted &&
                    (product.ShopOwnerId == ownerId || product.SupplierId == ownerId));
        }

        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedActiveProductsAsync(
            int page,
            int pageSize,
            Guid ownerId)
        {
            var query = _Context.Products
                                .AsNoTracking()
                                .Where(product => !product.IsDeleted &&
                                                  (product.ShopOwnerId == ownerId || product.SupplierId == ownerId))
                                .OrderBy(product => product.Name);

            var totalCount = await query.CountAsync();

            var items = await query.Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// Distinct suppliers that actually supply this shop's catalogue. Used by the
        /// retailer's Suppliers page so it never lists unrelated businesses.
        /// </summary>
        public async Task<IEnumerable<Guid>> GetSupplierIdsForShopAsync(Guid shopId)
        {
            return await _Context.Products
                                 .AsNoTracking()
                                 .Where(product => !product.IsDeleted &&
                                                   product.ShopOwnerId == shopId &&
                                                   product.SupplierId != null)
                                 .Select(product => product.SupplierId!.Value)
                                 .Distinct()
                                 .ToListAsync();
        }
    }
}

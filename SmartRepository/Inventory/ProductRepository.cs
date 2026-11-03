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

        public async Task<Product?> GetByBarcodeAsync(string barcode)
        {
            return await _Context.Products.FirstOrDefaultAsync(product => product.Barcode == barcode && !product.IsDeleted);
        }

        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedActiveProductsAsync(int page, int pageSize, Guid id)
        {
            var query = _Context.Products
                                .AsNoTracking()
                                .Where(product => !product.IsDeleted)
                                .OrderBy(product => product.Name);

            var totalCount = await query.CountAsync();

            var items = await query.Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return (items, totalCount);
        }
    }
}

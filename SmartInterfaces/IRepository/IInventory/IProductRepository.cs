using SmartModels;

namespace SmartInterfaces.IRepository.IInventory
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<Product?> GetByBarcodeAsync(string barcode, Guid ownerId);

        Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedActiveProductsAsync(int page, int pageSize, Guid ownerId);
    }
}

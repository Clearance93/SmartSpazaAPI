using SmartDto.Common;
using SmartDto.Inventory;

namespace SmartInterfaces.IService
{
    public interface IProductServices
    {
        Task<PagedResult<ProductDto>> GetPagedProductsAsync(PaginationParams pagination, Guid id);

        Task<ProductDto> GetProductByBarcodeAsync(string barcode);

        Task<ProductDto> GetProductByIdAsync(Guid productId);

        Task<ProductDto> CreateProductAsync(ProductDto dto);

        Task<ProductDto> UpdateProductAsync(Guid productId, ProductDto dto);

        Task<bool> DeleteProductAsync(Guid productId);

        Task<IEnumerable<ProductDto>> CheckoutAsync(CheckoutDto dto);
    }
}

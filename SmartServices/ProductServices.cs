using System.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartConfiguration;
using SmartCore.UnitOfWork;
using SmartDto.Common;
using SmartDto.Inventory;
using SmartInterfaces.IService;
using SmartModels;

namespace SmartServices
{
    public class ProductServices : IProductServices
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IMapper _Mapper;

        public ProductServices(IUnitOfWork unitOfWork,
                               IMapper mapper)
        {
            _UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PagedResult<ProductDto>> GetPagedProductsAsync(PaginationParams pagination, Guid id)
        {
            var (items, totalCount) = await _UnitOfWork.Product.GetPagedActiveProductsAsync(pagination.Page, pagination.PageSize, id);

            return new PagedResult<ProductDto>
            {
                Items = _Mapper.Map<IEnumerable<ProductDto>>(items),
                Page = pagination.Page,
                PageSize = pagination.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<ProductDto> GetProductByBarcodeAsync(string barcode, Guid ownerId)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                throw new ValidationException("A barcode is required to look up a product.");
            }

            var product = await _UnitOfWork.Product.GetByBarcodeAsync(barcode.Trim(), ownerId);

            if (product == null)
            {
                throw new NotFoundException($"No product was found with the barcode: {barcode}");
            }

            return _Mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> GetProductByIdAsync(Guid productId)
        {
            var product = await _UnitOfWork.Product.GetByIdAsync(productId);

            if (product == null || product.IsDeleted)
            {
                throw new NotFoundException($"The product with id: {productId} was not found");
            }

            return _Mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> CreateProductAsync(ProductDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Barcode))
            {
                throw new ValidationException("The barcode is required when creating a product.");
            }

            var barcode = dto.Barcode.Trim();
            var ownerId = dto.ShopOwnerId ?? dto.SupplierId ?? Guid.Empty;
            var existing = await _UnitOfWork.Product.GetByBarcodeAsync(barcode, ownerId);

            if (existing != null)
            {
                throw new ConflictException($"A product with the barcode: {barcode} already exists in your catalogue");
            }

            var product = _Mapper.Map<Product>(dto);

            product.ProductId = Guid.NewGuid();
            product.Barcode = barcode;
            product.IsDeleted = false;
            product.CreationTime = DateTime.UtcNow;
            product.Updated = null;

            await _UnitOfWork.Product.AddAsync(product);

            try
            {
                await _UnitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new ConflictException($"A product with the barcode: {barcode} already exists");
            }

            return _Mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> UpdateProductAsync(Guid productId, ProductDto dto)
        {
            var product = await _UnitOfWork.Product.GetByIdAsync(productId);

            if (product == null || product.IsDeleted)
            {
                throw new NotFoundException($"The product with id: {productId} was not found");
            }

            if (string.IsNullOrWhiteSpace(dto.Barcode))
            {
                throw new ValidationException("The barcode is required when updating a product.");
            }

            var barcode = dto.Barcode.Trim();
            var ownerId = product.ShopOwnerId ?? product.SupplierId ?? Guid.Empty;
            var duplicate = await _UnitOfWork.Product.GetByBarcodeAsync(barcode, ownerId);

            if (duplicate != null && duplicate.ProductId != productId)
            {
                throw new ConflictException($"A product with the barcode: {barcode} already exists in your catalogue");
            }

            product.Barcode = barcode;
            product.StockKeepUnit = dto.StockKeepUnit;
            product.Name = dto.Name;
            product.Category = dto.Category;
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.Unit = dto.Unit;
            product.Updated = DateTime.UtcNow;

            _UnitOfWork.Product.Update(product);

            try
            {
                await _UnitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new ConflictException($"A product with the barcode: {barcode} already exists");
            }

            return _Mapper.Map<ProductDto>(product);
        }

        public async Task<bool> DeleteProductAsync(Guid productId)
        {
            var product = await _UnitOfWork.Product.GetByIdAsync(productId);

            if (product == null || product.IsDeleted)
            {
                throw new NotFoundException($"The product with id: {productId} was not found");
            }

            product.IsDeleted = true;
            product.Updated = DateTime.UtcNow;

            _UnitOfWork.Product.Update(product);

            await _UnitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ProductDto>> CheckoutAsync(CheckoutDto dto)
        {
            if (dto == null || dto.Items == null || dto.Items.Count == 0)
            {
                throw new ValidationException("At least one item is required to complete a checkout.");
            }

            // Serializable isolation makes the read-check-decrement sequence atomic, so two
            // concurrent checkouts cannot both sell the same stock.
            await using var transaction = await _UnitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

            var updatedProducts = new List<Product>();

            foreach (var item in dto.Items)
            {
                if (item.Quantity <= 0)
                {
                    throw new ValidationException("Every checkout item must have a quantity greater than zero.");
                }

                var product = await ResolveProductAsync(item, dto.ShopOwnerId ?? Guid.Empty);

                if (product.StockQuantity < item.Quantity)
                {
                    throw new BusinessRuleException($"Only {product.StockQuantity} of {product.Name} remain in stock. " +
                                                    $"Cannot sell {item.Quantity}.");
                }

                product.StockQuantity -= item.Quantity;
                product.Updated = DateTime.UtcNow;

                if (!updatedProducts.Contains(product))
                {
                    updatedProducts.Add(product);
                }
            }

            foreach (var product in updatedProducts)
            {
                _UnitOfWork.Product.Update(product);
            }

            await _UnitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            return _Mapper.Map<IEnumerable<ProductDto>>(updatedProducts);
        }

        private async Task<Product> ResolveProductAsync(CheckoutItemDto item, Guid ownerId)
        {
            Product? product = null;

            if (item.ProductId != Guid.Empty)
            {
                product = await _UnitOfWork.Product.GetByIdAsync(item.ProductId);
            }

            if ((product == null || product.IsDeleted) && !string.IsNullOrWhiteSpace(item.Barcode))
            {
                product = await _UnitOfWork.Product.GetByBarcodeAsync(item.Barcode.Trim(), ownerId);
            }

            var ownsProduct = product != null &&
                              (product.ShopOwnerId == ownerId || product.SupplierId == ownerId);

            if (product == null || product.IsDeleted || !ownsProduct)
            {
                var reference = item.ProductId != Guid.Empty ? item.ProductId.ToString() : item.Barcode;

                throw new NotFoundException($"The product with reference: {reference} was not found");
            }

            return product;
        }
    }
}

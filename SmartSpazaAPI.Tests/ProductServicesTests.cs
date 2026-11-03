using AutoMapper;
using Moq;
using SmartConfiguration;
using SmartCore.UnitOfWork;
using SmartDto.Inventory;
using SmartInterfaces.IRepository.IInventory;
using SmartModels;
using SmartServices;
using Xunit;

namespace SmartSpazaAPI.Tests
{
    public class ProductServicesTests
    {
        private static (ProductServices Service, Mock<IProductRepository> ProductRepository) CreateService()
        {
            var productRepository = new Mock<IProductRepository>();

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(u => u.Product).Returns(productRepository.Object);

            var mapper = new Mock<IMapper>();

            return (new ProductServices(unitOfWork.Object, mapper.Object), productRepository);
        }

        [Fact]
        public async Task Checkout_WithNoItems_ThrowsValidationException()
        {
            var (service, _) = CreateService();

            await Assert.ThrowsAsync<ValidationException>(
                () => service.CheckoutAsync(new CheckoutDto { Items = new List<CheckoutItemDto>() }));
        }

        [Fact]
        public async Task GetProductById_WhenMissing_ThrowsNotFoundException()
        {
            var (service, productRepository) = CreateService();

            productRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                             .ReturnsAsync((Product?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => service.GetProductByIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateProduct_WithExistingBarcode_ThrowsConflict()
        {
            var (service, productRepository) = CreateService();

            productRepository.Setup(r => r.GetByBarcodeAsync(It.IsAny<string>()))
                             .ReturnsAsync(new Product { ProductId = Guid.NewGuid(), Barcode = "123" });

            await Assert.ThrowsAsync<ConflictException>(
                () => service.CreateProductAsync(new ProductDto { Barcode = "123", Name = "Test" }));
        }
    }
}

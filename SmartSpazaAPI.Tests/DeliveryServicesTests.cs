using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SmartCore.UnitOfWork;
using SmartDto.Delivery;
using SmartEnums;
using SmartInterfaces.IRepository;
using SmartInterfaces.IRepository.IBusiness;
using SmartInterfaces.IRepository.IDelivery;
using SmartInterfaces.IRepository.IDriver;
using SmartModels;
using SmartServices;
using Xunit;

namespace SmartSpazaAPI.Tests
{
    public class DeliveryServicesTests
    {
        [Fact]
        public async Task CreateDelivery_DefaultsToCreatedStatusAndGeneratesNumber()
        {
            var driverId = Guid.NewGuid();
            var supplierId = Guid.NewGuid();
            var shopId = Guid.NewGuid();

            var driverRepository = new Mock<IDriverRepostory>();
            driverRepository.Setup(repository => repository.GetByIdAsync(driverId))
                            .ReturnsAsync(new Drivers { DriverId = driverId });

            var supplierRepository = new Mock<ISupplierRepository>();
            supplierRepository.Setup(repository => repository.GetByIdAsync(supplierId))
                              .ReturnsAsync(new Supplier { SupplierId = supplierId });

            var shopRepository = new Mock<IShopOwnerRepository>();
            shopRepository.Setup(repository => repository.GetByIdAsync(shopId))
                          .ReturnsAsync(new ShopsOwner { ShopOwnerId = shopId });

            var deliveryRepository = new Mock<IDeliveryRepository>();
            Delivery? captured = null;
            deliveryRepository.Setup(repository => repository.AddAsync(It.IsAny<Delivery>()))
                              .Callback<Delivery>(delivery => captured = delivery)
                              .Returns(Task.CompletedTask);

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(work => work.Driver).Returns(driverRepository.Object);
            unitOfWork.SetupGet(work => work.Supplier).Returns(supplierRepository.Object);
            unitOfWork.SetupGet(work => work.ShopOwner).Returns(shopRepository.Object);
            unitOfWork.SetupGet(work => work.Delivery).Returns(deliveryRepository.Object);
            unitOfWork.Setup(work => work.SaveChangesAsync()).ReturnsAsync(1);

            var service = new DeliveryServices(unitOfWork.Object, CreateMapper());

            var result = await service.CreateDeliveryAsync(new DeliveryDto
            {
                DriverId = driverId,
                SupplierId = supplierId,
                ShopId = shopId,
                DeliveryNumber = "AB123"
            });

            Assert.True(result);
            Assert.NotNull(captured);
            Assert.Equal(DeliveryStatus.Created, captured!.DeliveryStatus);
            Assert.Equal(driverId, captured.DriverId);
            Assert.Equal(supplierId, captured.SupplierId);
            Assert.Equal(shopId, captured.ShopId);
            Assert.Equal("AB123", captured.DeliveryNumber);
            Assert.NotNull(captured.CreationTime);
        }

        private static IMapper CreateMapper()
        {
            var expression = new MapperConfigurationExpression();
            expression.AddProfile<SmartCore.AutoMapper>();

            return new MapperConfiguration(expression, NullLoggerFactory.Instance).CreateMapper();
        }
    }
}

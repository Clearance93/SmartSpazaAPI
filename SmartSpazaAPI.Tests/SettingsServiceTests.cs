using AutoMapper;
using Moq;
using SmartConfiguration;
using SmartCore.UnitOfWork;
using SmartDto.Driver;
using SmartInterfaces.IRepository;
using SmartInterfaces.IRepository.IDriver;
using SmartModels;
using SmartServices;
using Xunit;

namespace SmartSpazaAPI.Tests
{
    public class SettingsServiceTests
    {
        [Fact]
        public async Task AddDriverVehicle_WhenVehicleBelongsToAnotherDriver_ThrowsForbidden()
        {
            var driverId = Guid.NewGuid();
            var otherDriverId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();

            var driverRepository = new Mock<IDriverRepostory>();
            driverRepository.Setup(repository => repository.GetByIdAsync(driverId))
                            .ReturnsAsync(new Drivers { DriverId = driverId });

            var vehicleRepository = new Mock<IVehicleRepository>();
            vehicleRepository.Setup(repository => repository.GetByIdAsync(vehicleId))
                             .ReturnsAsync(new Vehicle
                             {
                                 Vid = vehicleId,
                                 DriverId = otherDriverId,
                                 VehicleName = "Other van"
                             });

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(work => work.Driver).Returns(driverRepository.Object);
            unitOfWork.SetupGet(work => work.Vehicle).Returns(vehicleRepository.Object);

            var service = new SettingsService(unitOfWork.Object, Mock.Of<IMapper>());

            await Assert.ThrowsAsync<ForbiddenException>(() => service.AddDriverVehicleAsync(new VehicleDto
            {
                Vid = vehicleId,
                DriverId = driverId,
                VehicleName = "My van",
                VehicleRegistration = "ABC123"
            }));
        }
    }
}

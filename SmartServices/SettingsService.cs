using AutoMapper;
using SmartConfiguration;
using SmartCore.UnitOfWork;
using SmartDto;
using SmartDto.Business;
using SmartDto.Driver;
using SmartInterfaces.IService;
using SmartModels;

namespace SmartServices
{
    public class SettingsService : ISettingServices
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IMapper _Mapper;

        public SettingsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> AddDriverVehicleAsync(VehicleDto dto)
        {
            var driver = await _UnitOfWork.Driver.GetByIdAsync(dto.DriverId);

            if (driver == null)
            {
                throw new NotFoundException($"The driver with the Id {dto.DriverId} was not found.");
            }

            var existingVehicle = dto.Vid == Guid.Empty
                ? null
                : await _UnitOfWork.Vehicle.GetByIdAsync(dto.Vid);

            if (existingVehicle == null)
            {
                var vehicle = _Mapper.Map<Vehicle>(dto);

                vehicle.Vid = Guid.NewGuid();
                vehicle.DriverId = dto.DriverId;
                vehicle.CreationTime = DateTime.UtcNow;

                await _UnitOfWork.Vehicle.AddAsync(vehicle);
                await _UnitOfWork.SaveChangesAsync();

                return true;
            }

            EnsureVehicleOwnership(existingVehicle, dto.DriverId);

            return await UpdateDriverVehicleAsync(dto);
        }

        public async Task<AvailabilityDto> GetAvailabilityByDriverIdAsync(Guid uid)
        {
            var availability = await _UnitOfWork.Availability.GetAvailabilityByUserIdAsync(uid);

            if (availability == null)
            {
                throw new NotFoundException($"The availability for driver {uid} was not found.");
            }

            return _Mapper.Map<AvailabilityDto>(availability);
        }

        public async Task<AvailabilityDto> UpdateAvailabilityAsync(Guid driverId, AvailabilityDto availabilityDto)
        {
            var availability = await _UnitOfWork.Availability.GetAvailabilityByUserIdAsync(driverId);

            if (availability == null)
            {
                throw new NotFoundException($"The availability for driver {driverId} was not found.");
            }

            availability.Monday = availabilityDto.Monday;
            availability.Tuesday = availabilityDto.Tuesday;
            availability.Wednesday = availabilityDto.Wednesday;
            availability.Thursday = availabilityDto.Thursday;
            availability.Friday = availabilityDto.Friday;
            availability.Saturday = availabilityDto.Saturday;
            availability.Sunday = availabilityDto.Sunday;
            availability.Updated = DateTime.UtcNow;

            await _UnitOfWork.SaveChangesAsync();

            return _Mapper.Map<AvailabilityDto>(availability);
        }

        public async Task<bool> UpdateDriverProfileAsync(Guid driverId, DriversDto dto)
        {
            var driver = await _UnitOfWork.Driver.GetByIdAsync(driverId);

            if (driver == null)
            {
                throw new NotFoundException($"The driver with the Id: {driverId} was not found.");
            }

            driver.FullName = dto.FullName;
            driver.Email = dto.Email;
            driver.PhoneNumbers = dto.PhoneNumbers;
            driver.WhatsAppNumber = dto.WhatsAppNumber;
            driver.DeliveryZone = dto.DeliveryZone;
            driver.ProfilePicture = dto.ProfilePicture;
            driver.Updated = DateTime.UtcNow;

            _UnitOfWork.Driver.Update(driver);

            await _UnitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateDriverVehicleAsync(VehicleDto dto)
        {
            var vehicle = await _UnitOfWork.Vehicle.GetByIdAsync(dto.Vid);

            if (vehicle == null)
            {
                throw new NotFoundException($"The vehicle with Id {dto.Vid} was not found.");
            }

            EnsureVehicleOwnership(vehicle, dto.DriverId);

            vehicle.VehicleName = dto.VehicleName;
            vehicle.VehicleRegistration = dto.VehicleRegistration;
            vehicle.LicenseNumber = dto.LicenseNumber;
            vehicle.Updated = DateTime.UtcNow;

            _UnitOfWork.Vehicle.Update(vehicle);

            await _UnitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<NavigationDto> UpdateNavigationAsync(Guid uid, NavigationDto navigationDto)
        {
            var navigation = await _UnitOfWork.Navigation.GetDriverNavigationAsync(uid);

            if (navigation == null)
            {
                throw new NotFoundException($"The navigation for driver {uid} was not found.");
            }

            navigation.OfflineMaps = navigationDto.OfflineMaps;
            navigation.AutoReRoute = navigationDto.AutoReRoute;
            navigation.TrafficAlertsOnMap = navigationDto.TrafficAlertsOnMap;
            navigation.VoiceGuidance = navigationDto.VoiceGuidance;
            navigation.Updated = DateTime.UtcNow;

            _UnitOfWork.Navigation.Update(navigation);

            await _UnitOfWork.SaveChangesAsync();

            return _Mapper.Map<NavigationDto>(navigation);
        }

        public async Task<bool> UpdateNotificationAsync(Guid uid, SystemNotifiationDto notificationDto)
        {
            var userNotification = await _UnitOfWork.SystemNotification.GetSystemNotificationAsync(uid);

            if (userNotification == null)
            {
                throw new NotFoundException($"The notification settings for driver {uid} were not found.");
            }

            userNotification.NewAssignments = notificationDto.NewAssignments;
            userNotification.TrafficAlerts = notificationDto.TrafficAlerts;
            userNotification.CustomerMessages = notificationDto.CustomerMessages;
            userNotification.SystemUpdate = notificationDto.SystemUpdate;
            userNotification.SoundAlerts = notificationDto.SoundAlerts;
            userNotification.Vibration = notificationDto.Vibration;
            userNotification.Updated = DateTime.UtcNow;

            _UnitOfWork.SystemNotification.Update(userNotification);

            await _UnitOfWork.SaveChangesAsync();

            return true;
        }

        private static void EnsureVehicleOwnership(Vehicle vehicle, Guid driverId)
        {
            if (vehicle.DriverId != driverId)
            {
                throw new ForbiddenException("You are not allowed to modify another driver's vehicle.");
            }
        }
    }
}

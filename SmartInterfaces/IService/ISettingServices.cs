using SmartDto;
using SmartDto.Business;
using SmartDto.Driver;

namespace SmartInterfaces.IService
{
    public interface ISettingServices
    {
        Task<AvailabilityDto> GetAvailabilityByDriverIdAsync(Guid uid);

        Task<AvailabilityDto> UpdateAvailabilityAsync(Guid uid, AvailabilityDto availabilityDto);

        Task<bool> UpdateNotificationAsync(Guid uid, SystemNotifiationDto notificationDto);

        Task<NavigationDto> UpdateNavigationAsync(Guid uid, NavigationDto navigationDto);

        Task<bool> UpdateDriverProfileAsync(Guid driverId, DriversDto dto);

        Task<bool> AddDriverVehicleAsync(VehicleDto dto);

        Task<bool> UpdateDriverVehicleAsync(VehicleDto dto);
    }
}

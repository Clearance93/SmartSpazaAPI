using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartConfiguration;
using SmartDto;
using SmartDto.Business;
using SmartDto.Driver;
using SmartInterfaces.IService;
using SmartSpazaAPI.Extensions;

namespace SmartSpazaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "DriverOnly")]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingServices _Settings;

        public SettingsController(ISettingServices settings)
        {
            _Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        private void EnsureOwnership(Guid driverId)
        {
            if (User.RequireRoleUserId() != driverId)
            {
                throw new ForbiddenException("You are not allowed to access another driver's settings.");
            }
        }

        [HttpGet("check")]
        public async Task<IActionResult> CheckAvailability(Guid uid)
        {
            EnsureOwnership(uid);

            var availability = await _Settings.GetAvailabilityByDriverIdAsync(uid);

            return Ok(availability);
        }

        [HttpPut("update-availability")]
        public async Task<IActionResult> UpdateAvailability(Guid uid, [FromBody] AvailabilityDto isAvailableDto)
        {
            EnsureOwnership(uid);

            await _Settings.UpdateAvailabilityAsync(uid, isAvailableDto);

            return Ok(new { message = "Availability updated successfully." });
        }

        [HttpPut("update-navigation")]
        public async Task<IActionResult> UpdateNavigation(Guid uid, [FromBody] NavigationDto navigationDto)
        {
            EnsureOwnership(uid);

            await _Settings.UpdateNavigationAsync(uid, navigationDto);

            return Ok(new { message = "Navigation updated successfully." });
        }

        [HttpPut("update-notifications")]
        public async Task<IActionResult> UpdateNotifications(Guid uid, [FromBody] SystemNotifiationDto notificationDto)
        {
            EnsureOwnership(uid);

            await _Settings.UpdateNotificationAsync(uid, notificationDto);

            return Ok(new { message = "Notifications updated successfully." });
        }

        [HttpPost("add-update-vehicle")]
        public async Task<IActionResult> AddVehicle([FromBody] VehicleDto dto)
        {
            EnsureOwnership(dto.DriverId);

            await _Settings.AddDriverVehicleAsync(dto);

            return Ok(new
            {
                message = dto.Vid == Guid.Empty ? "Vehicle added successfully." : "Vehicle updated successfully."
            });
        }

        [HttpPut("update-driver-info")]
        public async Task<IActionResult> UpdateDriverInfo(Guid driverId, [FromBody] DriversDto dto)
        {
            EnsureOwnership(driverId);

            await _Settings.UpdateDriverProfileAsync(driverId, dto);

            return Ok(new { message = "Driver information updated successfully." });
        }
    }
}

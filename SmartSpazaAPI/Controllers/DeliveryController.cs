using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartConfiguration;
using SmartDto.Delivery;
using SmartEnums;
using SmartInterfaces.IService;
using SmartSpazaAPI.Extensions;

namespace SmartSpazaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DeliveryController : ControllerBase
    {
        private readonly IDeliveryServices _Delivery;

        public DeliveryController(IDeliveryServices delivery)
        {
            _Delivery = delivery ?? throw new ArgumentNullException(nameof(delivery));
        }

        private Guid CurrentProfileId => User.RequireRoleUserId();

        private void EnsureDriverOwnership(Guid driverId)
        {
            if (CurrentProfileId != driverId)
            {
                throw new ForbiddenException("You are not allowed to access another driver's deliveries.");
            }
        }

        private async Task<DeliveryDto> GetDeliveryForParticipantAsync(Guid deliveryId)
        {
            var delivery = await _Delivery.GetDeliveryByIdAsync(deliveryId);
            var userId = CurrentProfileId;

            var isParticipant =
                (User.IsInRole(RoleNames.Supplier) && delivery.SupplierId == userId) ||
                (User.IsInRole(RoleNames.SpazaOwner) && delivery.ShopId == userId) ||
                (User.IsInRole(RoleNames.Driver) && delivery.DriverId == userId);

            if (!isParticipant)
            {
                throw new ForbiddenException("You are not allowed to access this delivery.");
            }

            return delivery;
        }

        [HttpPost("add-delivery")]
        [Authorize(Policy = "DriverOnly")]
        public async Task<IActionResult> AddDelivery([FromBody] DeliveryTrackingDto dto)
        {
            EnsureDriverOwnership(dto.DriverId);

            await _Delivery.CreateTrackingAsync(dto);

            return Ok(new
            {
                message = "Tracking was added successfully."
            });
        }

        [HttpGet("get-tracking")]
        [Authorize(Policy = "DriverOnly")]
        public async Task<IActionResult> GetTracking(Guid id)
        {
            var tracking = await _Delivery.GetTrackingAsync(id);

            EnsureDriverOwnership(tracking.DriverId);

            return Ok(tracking);
        }

        [HttpPut("update-tracking")]
        [Authorize(Policy = "DriverOnly")]
        public async Task<IActionResult> UpdateTracking(Guid id, [FromBody] DeliveryTrackingDto dto)
        {
            var tracking = await _Delivery.GetTrackingAsync(id);

            EnsureDriverOwnership(tracking.DriverId);

            await _Delivery.UpdateLocationAsync(id, dto);

            return Ok(new { message = "Tracking was updated successfully." });
        }

        [HttpPost("createDelivery")]
        [Authorize(Policy = "SupplierOnly")]
        public async Task<IActionResult> CreateNewDelivery([FromBody] DeliveryDto dto)
        {
            dto.SupplierId = CurrentProfileId;

            await _Delivery.CreateDeliveryAsync(dto);

            return Ok(new { message = "New delivery was added successfully." });
        }

        [HttpGet("allDeliveries")]
        [Authorize(Policy = "SupplierOnly")]
        public async Task<IActionResult> GetAllDeliveriesForSupplier(Guid id = default)
        {
            var supplierId = id == default ? CurrentProfileId : id;

            if (supplierId != CurrentProfileId)
            {
                throw new ForbiddenException("You are not allowed to access another supplier's deliveries.");
            }

            var deliveries = await _Delivery.GetAllDeliveriesForSupplierAsync(supplierId);

            return Ok(deliveries);
        }

        [HttpGet("driverDeliveries")]
        [HttpGet("driverDelivaries")]
        [Authorize(Policy = "DriverOnly")]
        public async Task<IActionResult> GetAllDriverDeliveries(Guid id)
        {
            EnsureDriverOwnership(id);

            var deliveries = await _Delivery.GetAllDeliveryByDriverIdAsync(id);

            return Ok(deliveries);
        }

        [HttpGet("deliveryById")]
        public async Task<IActionResult> GetDeliveryById(Guid id)
        {
            var delivery = await GetDeliveryForParticipantAsync(id);

            return Ok(delivery);
        }

        [HttpGet("getTheShop")]
        public async Task<IActionResult> GetTheShopById(Guid id)
        {
            var userId = CurrentProfileId;

            var allowed = (User.IsInRole(RoleNames.SpazaOwner) && id == userId)
                          || await _Delivery.CanAccessShopAsync(id, userId);

            if (!allowed)
            {
                throw new ForbiddenException("You are not allowed to access this shop.");
            }

            var shop = await _Delivery.GetShopOwnerByIdAsync(id);

            return Ok(shop);
        }

        [HttpGet("getSupplier")]
        public async Task<IActionResult> GetTheSupplierById(Guid id)
        {
            var userId = CurrentProfileId;

            var allowed = (User.IsInRole(RoleNames.Supplier) && id == userId)
                          || await _Delivery.CanAccessSupplierAsync(id, userId);

            if (!allowed)
            {
                throw new ForbiddenException("You are not allowed to access this supplier.");
            }

            var supplier = await _Delivery.GetSupplierByIdAsync(id);

            return Ok(supplier);
        }

        [HttpPut("updateDelivery")]
        [Authorize(Roles = RoleNames.Supplier + "," + RoleNames.Driver)]
        public async Task<IActionResult> UpdateTheDelivery(Guid id, [FromBody] DeliveryDto dto)
        {
            await GetDeliveryForParticipantAsync(id);

            await _Delivery.UpdateDeliveryAsync(id, dto);

            return Ok(new
            {
                message = "Delivery updated successfully."
            });
        }

        [HttpGet("driverDeliveriesTrack")]
        [Authorize(Policy = "DriverOnly")]
        public async Task<IActionResult> GetDriverDeliveryRecords(Guid id)
        {
            EnsureDriverOwnership(id);

            var records = await _Delivery.GetAllDeliveryRecordsAsync(id);

            return Ok(records);
        }

        [HttpGet("driversTracks")]
        [Authorize(Policy = "DriverOnly")]
        public async Task<IActionResult> GetDriversTrackingDelivery(Guid id)
        {
            EnsureDriverOwnership(id);

            var tracks = await _Delivery.GetTrackingByDriverAsync(id);

            return Ok(tracks);
        }

        [HttpPost("upsert")]
        [Authorize(Policy = "SpazaOwnerOnly")]
        public async Task<IActionResult> CreateOrUpdateRatings([FromBody] DeliveryRatingsDto dto, Guid id = default)
        {
            var shopId = CurrentProfileId;
            dto.ShopId = shopId;

            await _Delivery.UpsertAsync(dto, shopId, id);

            return Ok(new
            {
                message = id == Guid.Empty ? "Rating was added successfully." : "Rating was updated successfully."
            });
        }

        [HttpPost("upsert-delivery-item")]
        [Authorize(Policy = "SupplierOnly")]
        public async Task<IActionResult> UpsertDeliveryItem([FromBody] DeliveryItemsDto dto, Guid id = default)
        {
            if (id == Guid.Empty)
            {
                await GetDeliveryForParticipantAsync(dto.DeliveryId);
            }
            else
            {
                var item = await _Delivery.GetSingleDeliveryItemAsync(id);
                await GetDeliveryForParticipantAsync(item.DeliveryId);
            }

            await _Delivery.UpsertDeliveryItemAsync(dto, id);

            return Ok(new
            {
                message = id == Guid.Empty ? "Delivery item was added successfully." : "Delivery item was updated successfully."
            });
        }

        [HttpGet("delivery-item")]
        public async Task<IActionResult> GetDeliveryItem(Guid id)
        {
            var item = await _Delivery.GetSingleDeliveryItemAsync(id);
            await GetDeliveryForParticipantAsync(item.DeliveryId);

            return Ok(item);
        }

        [HttpGet("delivery-items")]
        public async Task<IActionResult> GetDeliveryItems(Guid deliveryId)
        {
            await GetDeliveryForParticipantAsync(deliveryId);

            var items = await _Delivery.GetAllDeliveryItemsByDeliveryId(deliveryId);

            return Ok(items);
        }
    }
}

using SmartDto.Business;
using SmartDto.Delivery;
using SmartEnums;

namespace SmartInterfaces.IService
{
    public interface IDeliveryServices
    {
        Task<bool> CreateTrackingAsync(DeliveryTrackingDto dto);

        Task<DeliveryTrackingDto> GetTrackingAsync(Guid trackingId);

        Task<IEnumerable<DeliveryTrackingDto>> GetTrackingByDriverAsync(Guid driverId);

        Task<bool> UpdateLocationAsync(Guid trackingId, DeliveryTrackingDto dto);

        Task<bool> CreateDeliveryAsync(DeliveryDto dto);

        Task<bool> UpdateDeliveryAsync(Guid id, DeliveryDto dto);

        Task<DeliveryDto> GetDeliveryByIdAsync(Guid deliveryId);

        Task<IEnumerable<DeliveryDto>> GetAllDeliveriesForSupplierAsync(Guid supplierId);

        Task<IEnumerable<DeliveryDto>> GetAllDeliveryByDriverIdAsync(Guid driverId);

        /// <summary>Purchase orders addressed to a shop, enriched for the retailer portal.</summary>
        Task<IEnumerable<ShopOrderDto>> GetShopOrdersAsync(Guid shopId);

        /// <summary>Suppliers whose products are in the shop's catalogue.</summary>
        Task<IEnumerable<ShopSupplierDto>> GetShopSuppliersAsync(Guid shopId);

        Task<SupplierDto> GetSupplierByIdAsync(Guid supplierId);

        Task<ShopOwnerDto> GetShopOwnerByIdAsync(Guid shopId);

        Task<bool> UpsertAsync(DeliveryRatingsDto dto, Guid shopId, Guid id = default);

        Task<IEnumerable<DeliveryRatingsDto>> GetAllDeliveryRecordsAsync(Guid driverId);

        Task<bool> UpsertDeliveryItemAsync(DeliveryItemsDto dto, Guid id = default);

        Task<DeliveryItemsDto> GetSingleDeliveryItemAsync(Guid id);

        Task<IEnumerable<DeliveryItemsDto>> GetAllDeliveryItemsByDeliveryId(Guid deliveryId);

        /// <summary>True when the user is a supplier, driver or the shop owner for at least one delivery to the shop.</summary>
        Task<bool> CanAccessShopAsync(Guid shopId, Guid userId);

        /// <summary>True when the user is the supplier or has a delivery from the supplier.</summary>
        Task<bool> CanAccessSupplierAsync(Guid supplierId, Guid userId);
    }
}

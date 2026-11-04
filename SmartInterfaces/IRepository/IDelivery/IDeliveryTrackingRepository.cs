using SmartDto.Delivery;
using SmartModels;

namespace SmartInterfaces.IRepository.IDelivery
{
    public interface IDeliveryTrackingRepository : IGenericRepository<DeliveryTracking>
    {
        Task<IEnumerable<DeliveryTracking>> GetTrackingByDriverAsync(Guid driverId);

        Task<IEnumerable<DeliveryTracking>> GetByDeliveryIdsAsync(IReadOnlyCollection<Guid> deliveryIds);
    }
}

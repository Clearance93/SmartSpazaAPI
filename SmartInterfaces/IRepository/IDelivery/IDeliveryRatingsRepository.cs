using SmartModels;

namespace SmartInterfaces.IRepository.IDelivery
{
    public interface IDeliveryRatingsRepository : IGenericRepository<DeliveryRatings>
    {
        Task<IEnumerable<DeliveryRatings>> GetByShopIdAsync(Guid shopId);

        Task<IEnumerable<DeliveryRatings>> GetByDriverIdAsync(Guid driverId);

        Task<IEnumerable<DeliveryRatings>> GetByDeliveryIdsAsync(IReadOnlyCollection<Guid> deliveryIds);
    }
}

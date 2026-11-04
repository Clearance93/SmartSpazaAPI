using SmartModels;

namespace SmartInterfaces.IRepository.IDelivery
{
    public interface IDeliveryRepository : IGenericRepository<Delivery>
    {
        Task<IEnumerable<Delivery>> GetBySupplierIdAsync(Guid supplierId);

        Task<IEnumerable<Delivery>> GetByShopIdAsync(Guid shopId);

        Task<IEnumerable<Delivery>> GetActiveByDriverIdAsync(Guid driverId);

        Task<IEnumerable<Delivery>> GetByDriverIdAsync(Guid driverId);
    }
}

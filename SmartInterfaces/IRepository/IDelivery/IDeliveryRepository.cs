using SmartModels;

namespace SmartInterfaces.IRepository.IDelivery
{
    public interface IDeliveryRepository : IGenericRepository<Delivery>
    {
        Task<IEnumerable<Delivery>> GetBySupplierIdAsync(Guid supplierId);

        Task<IEnumerable<Delivery>> GetActiveByDriverIdAsync(Guid driverId);
    }
}

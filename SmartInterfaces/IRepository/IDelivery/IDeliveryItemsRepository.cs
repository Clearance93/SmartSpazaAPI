using SmartModels;

namespace SmartInterfaces.IRepository.IDelivery
{
    public interface IDeliveryItemsRepository : IGenericRepository<DeliveryItems>
    {
        Task<IEnumerable<DeliveryItems>> GetAllDeliveryItemsByDeliveryId(Guid deliveryId);
    }
}

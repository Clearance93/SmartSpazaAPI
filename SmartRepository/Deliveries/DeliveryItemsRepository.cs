using SmartData;
using SmartInterfaces.IRepository.IDelivery;
using SmartModels;

namespace SmartRepository.Deliveries
{
    public class DeliveryItemsRepository : GenericRepository<DeliveryItems>, IDeliveryItemsRepository
    {
        public DeliveryItemsRepository(ApplicationDbContext context) : base(context)
        {
        }   
    }
}

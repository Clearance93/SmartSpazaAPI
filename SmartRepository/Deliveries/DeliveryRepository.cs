using SmartData;
using SmartInterfaces.IRepository.IDelivery;
using SmartModels;

namespace SmartRepository.Deliveries
{
    public class DeliveryRepository : GenericRepository<Delivery>, IDeliveryRepository
    {
        public DeliveryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

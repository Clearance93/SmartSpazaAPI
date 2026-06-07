using SmartData;
using SmartInterfaces.IRepository.IDelivery;
using SmartModels;

namespace SmartRepository.Deliveries
{
    public class DeliveryTrackingRepository : GenericRepository<DeliveryTracking>, IDeliveryTrackingRepository
    {
        public DeliveryTrackingRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

using SmartData;
using SmartInterfaces.IRepository.IDelivery;
using SmartModels;

namespace SmartRepository.Deliveries
{
    public class DeliveryRecordRepository : GenericRepository<DeliveryRecord>, IDeliveryRecordRepository
    {
        public DeliveryRecordRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

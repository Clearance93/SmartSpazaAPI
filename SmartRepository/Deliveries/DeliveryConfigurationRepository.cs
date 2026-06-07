using SmartData;
using SmartInterfaces.IRepository.IDelivery;
using SmartModels;

namespace SmartRepository.Deliveries
{
    public class DeliveryConfigurationRepository : GenericRepository<DeliveryConfiguration>, IDeliveryConfigurationRepository
    {
        public DeliveryConfigurationRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

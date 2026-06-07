using SmartData;
using SmartInterfaces.IRepository.IDelivery;
using SmartModels;

namespace SmartRepository.Deliveries
{
    public class DeliveryPreferenceRepository : GenericRepository<DeliveryPreference>, IDeliveryPreferenceRepository
    {
        public DeliveryPreferenceRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

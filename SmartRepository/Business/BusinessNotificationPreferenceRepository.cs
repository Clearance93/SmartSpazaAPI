using SmartData;
using SmartInterfaces.IRepository.IBusiness;
using SmartModels;

namespace SmartRepository.Business
{
    public class BusinessNotificationPreferenceRepository : GenericRepository<BusinessNotificationPreference>, IBusinessNotificationPreferenceRepository
    {
        public BusinessNotificationPreferenceRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
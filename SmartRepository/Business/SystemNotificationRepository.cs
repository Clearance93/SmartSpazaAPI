using SmartData;
using SmartInterfaces.IRepository.IBusiness;
using SmartModels;

namespace SmartRepository.Business
{
    public class SystemNotificationRepository : GenericRepository<SystemNotification>, ISystemNotificationRepository
    {
        public SystemNotificationRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

using Microsoft.EntityFrameworkCore;
using SmartData;
using SmartInterfaces.IRepository.IBusiness;
using SmartModels;

namespace SmartRepository.Business
{
    public class SystemNotificationRepository : GenericRepository<SystemNotification>, ISystemNotificationRepository
    {
        private readonly ApplicationDbContext _Context;
        public SystemNotificationRepository(ApplicationDbContext context) : base(context)
        {
            _Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<SystemNotification?> GetSystemNotificationAsync(Guid uid)
        {
            return await _Context.SystemNotification.FirstOrDefaultAsync(n => n.Uid == uid);
        }
    }
}

using SmartModels;

namespace SmartInterfaces.IRepository.IBusiness
{
    public interface ISystemNotificationRepository : IGenericRepository<SystemNotification>
    {
        Task<SystemNotification?> GetSystemNotificationAsync(Guid uid);
    }
}

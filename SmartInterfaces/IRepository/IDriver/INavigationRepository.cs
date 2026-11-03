using SmartModels;

namespace SmartInterfaces.IRepository.IDriver
{
    public interface INavigationRepository : IGenericRepository<Navigation>
    {
        Task<Navigation?> GetDriverNavigationAsync(Guid driverId);
    }
}

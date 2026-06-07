using SmartModels;

namespace SmartInterfaces.IRepository.IDriver
{
    public interface IDriverRepostory : IGenericRepository<Drivers>
    {
        Task<Drivers?> GetDriverByEmail(string email);
    }
}

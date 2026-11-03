using SmartModels;

namespace SmartInterfaces.IRepository
{
    public interface IAvailabilityRepository : IGenericRepository<Availability>
    {
        Task<Availability?> GetAvailabilityByUserIdAsync(Guid uid);
    }
}

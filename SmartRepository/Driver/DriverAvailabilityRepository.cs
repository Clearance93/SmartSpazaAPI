using SmartData;
using SmartInterfaces.IRepository.IDriver;
using SmartModels;

namespace SmartRepository.Driver
{
    public class DriverAvailabilityRepository : GenericRepository<DriversAvailability>, IDriverAvailabilityRepository
    {
        public DriverAvailabilityRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
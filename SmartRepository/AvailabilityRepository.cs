using SmartData;
using SmartInterfaces.IRepository;
using SmartModels;

namespace SmartRepository
{
    public class AvailabilityRepository : GenericRepository<Availability>, IAvailabilityRepository
    {
        public AvailabilityRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

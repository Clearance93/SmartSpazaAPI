using SmartData;
using SmartInterfaces.IRepository.IBusiness;
using SmartModels;

namespace SmartRepository.Business
{
    public class OperatingHoursRepository : GenericRepository<OperatingHours>, IOperatingHoursRepository
    {
        public OperatingHoursRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

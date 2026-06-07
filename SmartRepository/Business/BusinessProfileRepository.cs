using SmartData;
using SmartInterfaces.IRepository.IBusiness;
using SmartModels;

namespace SmartRepository.Business
{
    public class BusinessProfileRepository : GenericRepository<BusinessProfile>, IBusinessProfileRepository
    {
        public BusinessProfileRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

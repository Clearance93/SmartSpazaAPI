using SmartData;
using SmartInterfaces.IRepository.IBusiness;
using SmartModels;

namespace SmartRepository.Business
{
    public class SupplierBusinessInfoRepository : GenericRepository<SupplierBusinessInfo>, ISupplierBusinessInfoRepository
    {
        public SupplierBusinessInfoRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

using SmartData;
using SmartInterfaces.IRepository.IBusiness;
using SmartModels;

namespace SmartRepository.Business
{
    public class SupplierBusinessNotofocationRepository : GenericRepository<SupplierBusinessNotification>, ISupplierBusinessNotofocationRepository
    {
        public SupplierBusinessNotofocationRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

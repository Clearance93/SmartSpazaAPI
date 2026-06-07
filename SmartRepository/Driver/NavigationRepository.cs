using SmartData;
using SmartInterfaces.IRepository.IDriver;
using SmartModels;

namespace SmartRepository.Driver
{
    public class NavigationRepository : GenericRepository<Navigation>, INavigationRepository
    {
        public NavigationRepository(ApplicationDbContext context) : base(context)
        {
        }   
    }
}

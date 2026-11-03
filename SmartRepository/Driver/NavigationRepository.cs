using Microsoft.EntityFrameworkCore;
using SmartData;
using SmartInterfaces.IRepository.IDriver;
using SmartModels;

namespace SmartRepository.Driver
{
    public class NavigationRepository : GenericRepository<Navigation>, INavigationRepository
    {
        private readonly ApplicationDbContext _Context;

        public NavigationRepository(ApplicationDbContext context) : base(context)
        {
            _Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Navigation?> GetDriverNavigationAsync(Guid driverId)
        {
            return await _Context.Navigation.FirstOrDefaultAsync(navigation => navigation.DriverId == driverId);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using SmartData;
using SmartInterfaces.IRepository.IDriver;
using SmartModels;

namespace SmartRepository.Driver
{
    public class DriverRepostory : GenericRepository<Drivers>, IDriverRepostory
    {
        private readonly ApplicationDbContext _Context;
        public DriverRepostory(ApplicationDbContext context) : base(context)
        { 
            _Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Drivers?> GetDriverByEmail(string email)
        {
            var driver = await _Context.Drivers.FirstOrDefaultAsync(d => d.Email == email);
            
            return driver;
        }
    }
}

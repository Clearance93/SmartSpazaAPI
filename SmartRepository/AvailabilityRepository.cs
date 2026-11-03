using Microsoft.EntityFrameworkCore;
using SmartData;
using SmartInterfaces.IRepository;
using SmartModels;

namespace SmartRepository
{
    public class AvailabilityRepository : GenericRepository<Availability>, IAvailabilityRepository
    {
        private readonly ApplicationDbContext _Context;

        public AvailabilityRepository(ApplicationDbContext context) : base(context)
        {
            _Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Availability?> GetAvailabilityByUserIdAsync(Guid uid)
        {
            return await _Context.Availability.FirstOrDefaultAsync(availability => availability.Uid == uid);
        }
    }
}

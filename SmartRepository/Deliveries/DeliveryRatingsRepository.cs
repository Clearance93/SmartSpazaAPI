using Microsoft.EntityFrameworkCore;
using SmartData;
using SmartInterfaces.IRepository.IDelivery;
using SmartModels;

namespace SmartRepository.Deliveries
{
    public class DeliveryRatingsRepository : GenericRepository<DeliveryRatings>, IDeliveryRatingsRepository
    {
        private readonly ApplicationDbContext _Context;

        public DeliveryRatingsRepository(ApplicationDbContext context) : base(context)
        {
            _Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<DeliveryRatings>> GetByShopIdAsync(Guid shopId)
        {
            return await _Context.DeliveryRatings
                                 .AsNoTracking()
                                 .Where(rating => rating.ShopId == shopId)
                                 .OrderByDescending(rating => rating.Created)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<DeliveryRatings>> GetByDriverIdAsync(Guid driverId)
        {
            return await _Context.DeliveryRatings
                                 .AsNoTracking()
                                 .Where(rating => rating.DriverId == driverId)
                                 .OrderByDescending(rating => rating.Created)
                                 .ToListAsync();
        }
    }
}

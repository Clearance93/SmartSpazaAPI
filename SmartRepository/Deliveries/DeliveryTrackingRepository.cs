using Microsoft.EntityFrameworkCore;
using SmartData;
using SmartInterfaces.IRepository.IDelivery;
using SmartModels;

namespace SmartRepository.Deliveries
{
    public class DeliveryTrackingRepository : GenericRepository<DeliveryTracking>, IDeliveryTrackingRepository
    {
        private readonly ApplicationDbContext _Context;

        public DeliveryTrackingRepository(ApplicationDbContext context) : base(context)
        {
            _Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<DeliveryTracking>> GetTrackingByDriverAsync(Guid driverId)
        {
            return await _Context.DeliveryTracking
                                 .AsNoTracking()
                                 .Where(tracking => tracking.DriverId == driverId)
                                 .OrderByDescending(tracking => tracking.Updated ?? tracking.CreationTime)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<DeliveryTracking>> GetByDeliveryIdsAsync(IReadOnlyCollection<Guid> deliveryIds)
        {
            if (deliveryIds.Count == 0)
            {
                return Array.Empty<DeliveryTracking>();
            }

            return await _Context.DeliveryTracking
                                 .AsNoTracking()
                                 .Where(tracking => tracking.DeliveryId.HasValue &&
                                                    deliveryIds.Contains(tracking.DeliveryId.Value))
                                 .OrderByDescending(tracking => tracking.Updated ?? tracking.CreationTime)
                                 .ToListAsync();
        }
    }
}

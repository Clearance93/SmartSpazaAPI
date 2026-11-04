using Microsoft.EntityFrameworkCore;
using SmartData;
using SmartEnums;
using SmartInterfaces.IRepository.IDelivery;
using SmartModels;

namespace SmartRepository.Deliveries
{
    public class DeliveryRepository : GenericRepository<Delivery>, IDeliveryRepository
    {
        private readonly ApplicationDbContext _Context;

        public DeliveryRepository(ApplicationDbContext context) : base(context)
        {
            _Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Delivery>> GetBySupplierIdAsync(Guid supplierId)
        {
            return await _Context.Deliveries
                                 .AsNoTracking()
                                 .Where(delivery => delivery.SupplierId == supplierId)
                                 .OrderByDescending(delivery => delivery.CreationTime)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Delivery>> GetByShopIdAsync(Guid shopId)
        {
            return await _Context.Deliveries
                                 .AsNoTracking()
                                 .Where(delivery => delivery.ShopId == shopId)
                                 .OrderByDescending(delivery => delivery.CreationTime)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Delivery>> GetActiveByDriverIdAsync(Guid driverId)
        {
            return await _Context.Deliveries
                                 .AsNoTracking()
                                 .Where(delivery => delivery.DriverId == driverId &&
                                                    delivery.DeliveryStatus != DeliveryStatus.Delivered &&
                                                    delivery.DeliveryStatus != DeliveryStatus.Rejected)
                                 .OrderByDescending(delivery => delivery.CreationTime)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Delivery>> GetByDriverIdAsync(Guid driverId)
        {
            return await _Context.Deliveries
                                 .AsNoTracking()
                                 .Where(delivery => delivery.DriverId == driverId)
                                 .OrderByDescending(delivery => delivery.CreationTime)
                                 .ToListAsync();
        }
    }
}

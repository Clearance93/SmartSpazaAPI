using Microsoft.EntityFrameworkCore;
using SmartData;
using SmartInterfaces.IRepository.IDelivery;
using SmartModels;

namespace SmartRepository.Deliveries
{
    public class DeliveryItemsRepository : GenericRepository<DeliveryItems>, IDeliveryItemsRepository
    {
        private readonly ApplicationDbContext _Context;

        public DeliveryItemsRepository(ApplicationDbContext context) : base(context)
        {
            _Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<DeliveryItems>> GetAllDeliveryItemsByDeliveryId(Guid deliveryId)
        {
            return await _Context.DeliveryItems.Where(d => d.DeliveryId == deliveryId)
                                               .ToListAsync();
        }
    }
}

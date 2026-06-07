using Microsoft.EntityFrameworkCore;
using SmartData;
using SmartInterfaces.IRepository.IBusiness;
using SmartModels;

namespace SmartRepository.Business
{
    public class ShopOwnerRepository : GenericRepository<ShopsOwner>, IShopOwnerRepository
    {
        private readonly ApplicationDbContext _Context;
        public ShopOwnerRepository(ApplicationDbContext context) : base(context)
        {
            _Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ShopsOwner?> GetShopOwnerByEmail(string email)
        {
            return await _Context.ShopsOwners.FirstOrDefaultAsync(s => s.Email == email);
        }
    }
}

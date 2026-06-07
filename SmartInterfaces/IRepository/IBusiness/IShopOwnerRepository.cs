using SmartModels;

namespace SmartInterfaces.IRepository.IBusiness
{
    public interface IShopOwnerRepository : IGenericRepository<ShopsOwner>
    {
        Task<ShopsOwner?> GetShopOwnerByEmail(string email);
    }
}

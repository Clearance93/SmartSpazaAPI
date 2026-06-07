using SmartModels;

namespace SmartInterfaces.IRepository.IBusiness
{
    public interface ISupplierRepository : IGenericRepository<Supplier>
    {
        Task<Supplier?> GetSupplierByEmail(string email);
    }
}

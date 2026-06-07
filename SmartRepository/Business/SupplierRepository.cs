using Microsoft.EntityFrameworkCore;
using SmartData;
using SmartInterfaces.IRepository.IBusiness;
using SmartModels;

namespace SmartRepository.Business
{
    public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
    {
        private readonly ApplicationDbContext _Context;

        public SupplierRepository(ApplicationDbContext context) : base (context) 
        {
            _Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Supplier?> GetSupplierByEmail(string email)
        {
            return await _Context.Suppliers.FirstOrDefaultAsync(s => s.Email == email);
        }
    }
}

using SmartData;
using SmartInterfaces.IRepository.IDriver;
using SmartModels;

namespace SmartRepository.Driver
{
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

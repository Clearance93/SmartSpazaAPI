using SmartData;
using SmartInterfaces.IRepository.IBusiness;
using SmartModels;

namespace SmartRepository.Business
{
    public class BusinessTeamMembersRepository : GenericRepository<BusinessTeamMembers>, IBusinesTeamMembersRepository
    {
        public BusinessTeamMembersRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}

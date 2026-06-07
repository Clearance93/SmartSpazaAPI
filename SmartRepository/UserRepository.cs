using Microsoft.EntityFrameworkCore;
using SmartData;
using SmartInterfaces.IRepository;
using SmartModels;

namespace SmartRepository
{
    public class UserRepository : GenericRepository<ApplicationUser>, IUserRepository
    {
        private readonly ApplicationDbContext _Context;

        public UserRepository(ApplicationDbContext context) : base(context)
        {
            _Context = context ??  throw new ArgumentNullException(nameof(context));
        }

        public Task<ApplicationUser?> CheckEmailConfirmationAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ApplicationUser>> GetAllActiveUserAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await _Context.User.FirstOrDefaultAsync(u => u.Email == email);
        }

        public Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<string?> GetUserRole(string role)
        {
            return await _Context.Roles.Where(r => r.Name == role)
                                       .Select(r => r.Name)
                                       .FirstOrDefaultAsync();
        }

        public Task<string?> GetUserRoleByUserIdAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
}

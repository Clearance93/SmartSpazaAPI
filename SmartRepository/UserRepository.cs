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

        public async Task<ApplicationUser?> CheckEmailConfirmationAsync(string userId)
        {
            return await _Context.User.FirstOrDefaultAsync(u => u.Id == userId && u.EmailConfirmed);
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllActiveUserAsync()
        {
            return await _Context.User
                .Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow)
                .ToListAsync();
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await _Context.User.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _Context.User.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<string?> GetUserRole(string role)
        {
            return await _Context.Roles.Where(r => r.Name == role)
                                       .Select(r => r.Name)
                                       .FirstOrDefaultAsync();
        }

        public async Task<string?> GetUserRoleByUserIdAsync(string userId)
        {
            return await (from userRole in _Context.UserRoles
                          join role in _Context.Roles on userRole.RoleId equals role.Id
                          where userRole.UserId == userId
                          select role.Name).FirstOrDefaultAsync();
        }
    }
}

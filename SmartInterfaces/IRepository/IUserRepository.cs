using SmartModels;

namespace SmartInterfaces.IRepository
{
    public interface IUserRepository : IGenericRepository<ApplicationUser>
    {
        Task<ApplicationUser?> GetUserByEmailAsync(string email);

        Task<ApplicationUser?> GetUserByIdAsync(string userId);

        Task<IEnumerable<ApplicationUser>> GetAllActiveUserAsync();

        Task<string?> GetUserRole(string role);

        Task<string?> GetUserRoleByUserIdAsync(string email);

        Task<ApplicationUser?> CheckEmailConfirmationAsync(string userId);
    }
}

using SmartDto;

namespace SmartInterfaces.IService
{
    public interface IUserService
    {
        Task<ResponseUserDto> CreateUserAsync(UserDto dto);

        Task<UserDto> UpdateUserAsync(string userId, UserDto dto, string token = null!);

        Task<ResponseUserDto> AuthenticateUserAsync(UserLoginDto dto);

        Task<string> GeneratePasswordResetTOkenRepositoryAsnc(string email);

        Task<bool> EmailConfirmationAsync(string userId, string token);

        Task<bool> ChangePasswordAsync(PasswordChangeDto dto);
    }
}

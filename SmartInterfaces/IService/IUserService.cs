using SmartDto;

namespace SmartInterfaces.IService
{
    public interface IUserService
    {
        Task<ResponseUserDto> CreateUserAsync(RegisterDto dto);

        Task<ResponseUserDto> AuthenticateUserAsync(UserLoginDto dto, string? ipAddress = null);

        Task<ResponseUserDto> RefreshTokenAsync(string refreshToken, string? ipAddress);

        Task<bool> RevokeRefreshTokenAsync(string refreshToken, string? ipAddress);

        Task<bool> EmailConfirmationAsync(string userId, string token);

        Task<bool> ResendConfirmationEmailAsync(string email);

        Task<bool> ForgotPasswordAsync(string email);

        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);

        Task<bool> ChangePasswordAsync(string userId, PasswordChangeDto dto);
    }
}

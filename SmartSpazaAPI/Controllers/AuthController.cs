using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartDto;
using SmartInterfaces.IService;
using SmartSpazaAPI.Extensions;

namespace SmartSpazaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _UserService;

        public AuthController(IUserService userService)
        {
            _UserService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        private string? IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {
            var result = await _UserService.AuthenticateUserAsync(userLoginDto, IpAddress);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("registration")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _UserService.CreateUserAsync(registerDto);

            var message = result.EmailConfirmed
                ? "Registration successful. Your account is ready — you can sign in now."
                : result.ConfirmationEmailSent
                    ? "Registration successful. Please check your email to confirm your account before logging in."
                    : "Registration successful, but the confirmation email could not be sent. Use resend-confirmation to try again before signing in.";

            return Ok(new
            {
                message,
                email = result.Email,
                roles = result.Roles,
                emailConfirmed = result.EmailConfirmed,
                confirmationEmailSent = result.ConfirmationEmailSent
            });
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var result = await _UserService.RefreshTokenAsync(refreshTokenDto.RefreshToken!, IpAddress);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            await _UserService.RevokeRefreshTokenAsync(refreshTokenDto.RefreshToken!, IpAddress);

            return Ok(new { message = "Refresh token revoked." });
        }

        [AllowAnonymous]
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmailLink([FromQuery] string userId, [FromQuery] string token)
        {
            await _UserService.EmailConfirmationAsync(userId, token);

            return Ok(new { message = "Email confirmed successfully. You can now log in." });
        }

        [AllowAnonymous]
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            await _UserService.EmailConfirmationAsync(userId, token);

            return Ok(new { message = "Email confirmed successfully. You can now log in." });
        }

        [AllowAnonymous]
        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] EmailRequestDto request)
        {
            await _UserService.ResendConfirmationEmailAsync(request.Email!);

            return Ok(new { message = "If that account exists and is unconfirmed, a new confirmation email has been sent." });
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] EmailRequestDto request)
        {
            await _UserService.ForgotPasswordAsync(request.Email!);

            return Ok(new { message = "If that account exists, a password reset link has been sent to the registered email address." });
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            await _UserService.ResetPasswordAsync(resetPasswordDto);

            return Ok(new { message = "Password reset successfully. You can now log in." });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeDto passwordChangeDto)
        {
            await _UserService.ChangePasswordAsync(User.RequireUserId(), passwordChangeDto);

            return Ok(new { message = "Password changed successfully." });
        }
    }
}

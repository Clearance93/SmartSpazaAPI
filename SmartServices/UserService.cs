using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SmartConfiguration;
using SmartCore.UnitOfWork;
using SmartDto;
using SmartDto.Business;
using SmartDto.Driver;
using SmartEnums;
using SmartInterfaces.IService;
using SmartModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmartServices
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IMapper _Mapper;
        private readonly IConfiguration _Configuration;
        private readonly UserManager<ApplicationUser> _UserManager;
        private readonly RoleManager<IdentityRole> _RoleManager;
        private readonly IConfirmationEmailSender _ConfirmationEmailSender;
        private readonly ILogger<UserService> _Logger;

        public UserService(IUnitOfWork unitOfWork,
                           IMapper mapper,
                           IConfiguration configuration,
                           UserManager<ApplicationUser> userManager,
                           RoleManager<IdentityRole> roleManager,
                           IConfirmationEmailSender confirmationEmailSender,
                           ILogger<UserService> logger)
        {
            _UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _RoleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _ConfirmationEmailSender = confirmationEmailSender ?? throw new ArgumentNullException(nameof(confirmationEmailSender));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResponseUserDto> AuthenticateUserAsync(UserLoginDto dto, string? ipAddress = null)
        {
            var user = await _UserManager.FindByEmailAsync(dto.Email!);

            if (user == null)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            if (await _UserManager.IsLockedOutAsync(user))
            {
                _Logger.LogWarning("Login attempt for locked account {Email}.", dto.Email);

                throw new UnauthorizedException("Invalid email or password.");
            }

            var passwordValid = await _UserManager.CheckPasswordAsync(user, dto.Password!);

            if (!passwordValid)
            {
                await _UserManager.AccessFailedAsync(user);

                _Logger.LogWarning("Failed login attempt for {Email}.", dto.Email);

                throw new UnauthorizedException("Invalid email or password.");
            }

            if (!user.EmailConfirmed)
            {
                throw new UnauthorizedException("Email not confirmed. Please confirm your email before logging in.");
            }

            await _UserManager.ResetAccessFailedCountAsync(user);

            _Logger.LogInformation("User {Email} authenticated successfully.", user.Email);

            return await IssueTokensAsync(user, ipAddress);
        }

        public async Task<ResponseUserDto> RefreshTokenAsync(string refreshToken, string? ipAddress)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ValidationException("A refresh token is required.");
            }

            var tokenHash = HashToken(refreshToken);
            var existing = await _UnitOfWork.RefreshToken.GetByTokenHashAsync(tokenHash);

            if (existing == null)
            {
                throw new UnauthorizedException("Invalid refresh token.");
            }

            if (!existing.IsActive)
            {
                _Logger.LogWarning("Refresh token reuse detected for user {UserId}; revoking all active tokens.", existing.UserId);

                await RevokeAllActiveTokensAsync(existing.UserId, ipAddress);

                throw new UnauthorizedException("Invalid refresh token.");
            }

            var user = await _UserManager.FindByIdAsync(existing.UserId);

            if (user == null || !user.EmailConfirmed)
            {
                await RevokeAllActiveTokensAsync(existing.UserId, ipAddress);

                throw new UnauthorizedException("Invalid refresh token.");
            }

            var newRefreshTokenValue = GenerateRefreshTokenValue();
            var newRefreshTokenHash = HashToken(newRefreshTokenValue);

            // Claim the old token atomically. A concurrent refresh with the same token will
            // fail here and trigger full token revocation instead of issuing a second session.
            var revoked = await _UnitOfWork.RefreshToken.TryRevokeAsync(existing.RefreshTokenId,
                                                                        DateTime.UtcNow,
                                                                        ipAddress,
                                                                        newRefreshTokenHash);

            if (!revoked)
            {
                _Logger.LogWarning("Concurrent refresh detected for user {UserId}; revoking all active tokens.", existing.UserId);

                await RevokeAllActiveTokensAsync(existing.UserId, ipAddress);

                throw new UnauthorizedException("Invalid refresh token.");
            }

            return await IssueTokensAsync(user,
                                          ipAddress,
                                          newRefreshTokenValue,
                                          newRefreshTokenHash);
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, string? ipAddress)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return true;
            }

            var existing = await _UnitOfWork.RefreshToken.GetByTokenHashAsync(HashToken(refreshToken));

            if (existing == null || !existing.IsActive)
            {
                return true;
            }

            await _UnitOfWork.RefreshToken.TryRevokeAsync(existing.RefreshTokenId,
                                                          DateTime.UtcNow,
                                                          ipAddress,
                                                          null);

            return true;
        }

        public async Task<ResponseUserDto> CreateUserAsync(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                throw new ValidationException("Email and password are required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Role) || !RoleNames.All.Contains(dto.Role))
            {
                throw new ValidationException("Specified role is not supported for registration.");
            }

            var existingUser = await _UserManager.FindByEmailAsync(dto.Email);

            if (existingUser != null)
            {
                throw new ConflictException("A user with this email already exists.");
            }

            var user = _Mapper.Map<ApplicationUser>(dto);

            user.Id = Guid.NewGuid().ToString();
            user.UserName = dto.Email;
            user.Email = dto.Email;
            user.PhoneNumber = dto.WhatsAppNumber;
            user.CreationTime = DateTime.UtcNow;

            var result = await _UserManager.CreateAsync(user, dto.Password!);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));

                throw new ValidationException($"Failed to create user: {errors}");
            }

            if (!await _RoleManager.RoleExistsAsync(dto.Role))
            {
                await _RoleManager.CreateAsync(new IdentityRole(dto.Role));
            }

            var addToRole = await _UserManager.AddToRoleAsync(user, dto.Role);

            if (!addToRole.Succeeded)
            {
                await _UserManager.DeleteAsync(user);

                var errors = string.Join(", ", addToRole.Errors.Select(e => e.Description));

                throw new ValidationException($"Failed to assign role to user: {errors}");
            }

            try
            {
                await CreateRoleProfileAsync(user, dto);
            }
            catch
            {
                await _UserManager.DeleteAsync(user);

                throw;
            }

            var requireEmailConfirmation = _Configuration.GetValue("Auth:RequireEmailConfirmation", true);
            var confirmationEmailSent = false;

            if (requireEmailConfirmation)
            {
                var confirmationToken = await _UserManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken));

                try
                {
                    await _ConfirmationEmailSender.SendConfirmationAsync(user.Email!, user.Id, encodedToken);
                    confirmationEmailSent = true;
                }
                catch (Exception exception)
                {
                    // The account exists and can be confirmed later via resend-confirmation.
                    // Do not turn a transient mail failure into a half-created account error.
                    _Logger.LogError(exception,
                                     "The confirmation email for {Email} could not be sent.",
                                     user.Email);
                }
            }
            else
            {
                // Email confirmation is switched off (e.g. no SMTP in local development),
                // so the account is usable straight away.
                user.EmailConfirmed = true;

                var updateResult = await _UserManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));

                    throw new ValidationException($"Failed to confirm new user: {errors}");
                }

                _Logger.LogWarning("Auth:RequireEmailConfirmation is disabled. {Email} was registered as already confirmed.", user.Email);
            }

            return new ResponseUserDto
            {
                Email = user.Email,
                Roles = new List<string> { dto.Role },
                EmailConfirmed = user.EmailConfirmed,
                ConfirmationEmailSent = confirmationEmailSent || !requireEmailConfirmation
            };
        }

        private async Task CreateRoleProfileAsync(ApplicationUser user, RegisterDto dto)
        {
            switch (dto.Role)
            {
                case RoleNames.Supplier:
                    var supplier = _Mapper.Map<Supplier>(dto);
                    supplier.SupplierId = Guid.Parse(user.Id);
                    supplier.CreatedAt = user.CreationTime;
                    await _UnitOfWork.Supplier.AddAsync(supplier);
                    break;

                case RoleNames.SpazaOwner:
                    var shopOwner = _Mapper.Map<ShopsOwner>(dto);
                    shopOwner.ShopOwnerId = Guid.Parse(user.Id);
                    shopOwner.ShopType = ShopType.General;
                    shopOwner.CreatedAt = user.CreationTime;
                    await _UnitOfWork.ShopOwner.AddAsync(shopOwner);
                    break;

                case RoleNames.Driver:
                    var driver = _Mapper.Map<Drivers>(dto);
                    driver.DriverId = Guid.Parse(user.Id);
                    driver.CreationTime = user.CreationTime;
                    await _UnitOfWork.Driver.AddAsync(driver);

                    await _UnitOfWork.Availability.AddAsync(_Mapper.Map<Availability>(new AvailabilityDto
                    {
                        AvailabilityId = Guid.NewGuid(),
                        Uid = driver.DriverId,
                        CreationTime = DateTime.UtcNow
                    }));

                    await _UnitOfWork.Navigation.AddAsync(_Mapper.Map<Navigation>(new NavigationDto
                    {
                        NavigationId = Guid.NewGuid(),
                        DriverId = driver.DriverId,
                        CreationTime = DateTime.UtcNow
                    }));

                    await _UnitOfWork.SystemNotification.AddAsync(_Mapper.Map<SystemNotification>(new SystemNotifiationDto
                    {
                        SystemNotificationId = Guid.NewGuid(),
                        Uid = driver.DriverId,
                        NewAssignments = true,
                        CustomerMessages = true,
                        SystemUpdate = true,
                        CreationTime = DateTime.UtcNow
                    }));
                    break;
            }

            await _UnitOfWork.SaveChangesAsync();
        }

        public async Task<bool> EmailConfirmationAsync(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                throw new ValidationException("User ID and token are required.");
            }

            var user = await _UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new ValidationException("Invalid confirmation link.");
            }

            if (user.EmailConfirmed)
            {
                return true;
            }

            var decodedToken = DecodeToken(token);

            var result = await _UserManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                throw new ValidationException("This confirmation link is invalid or has expired. Please request a new confirmation email.");
            }

            return true;
        }

        public async Task<bool> ResendConfirmationEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ValidationException("Email is required.");
            }

            if (!_Configuration.GetValue("Auth:RequireEmailConfirmation", true))
            {
                return true;
            }

            var user = await _UserManager.FindByEmailAsync(email);

            if (user == null || user.EmailConfirmed)
            {
                return true;
            }

            var token = await _UserManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            try
            {
                await _ConfirmationEmailSender.SendConfirmationAsync(user.Email!, user.Id, encodedToken);
            }
            catch (Exception exception)
            {
                // Keep the response identical whether or not the account exists or mail is down.
                _Logger.LogError(exception, "The confirmation email for {Email} could not be sent.", user.Email);
            }

            return true;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ValidationException("Email is required.");
            }

            var user = await _UserManager.FindByEmailAsync(email);

            if (user == null)
            {
                return true;
            }

            var token = await _UserManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            try
            {
                await _ConfirmationEmailSender.SendPasswordResetAsync(user.Email!, encodedToken);
            }
            catch (Exception exception)
            {
                _Logger.LogError(exception, "The password reset email for {Email} could not be sent.", user.Email);
            }

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Token) ||
                string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                throw new ValidationException("Email, token and new password are required.");
            }

            var user = await _UserManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                throw new ValidationException("This password reset request is invalid or has expired.");
            }

            var decodedToken = DecodeToken(dto.Token);
            var result = await _UserManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));

                throw new ValidationException($"Failed to reset password: {errors}");
            }

            await RevokeAllActiveTokensAsync(user.Id, null);

            _Logger.LogInformation("Password reset completed for user {UserId}.", user.Id);

            return true;
        }

        public async Task<bool> ChangePasswordAsync(string userId, PasswordChangeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.OldPassword) ||
                string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                throw new ValidationException("Current password and new password are required.");
            }

            var user = await _UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            var result = await _UserManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));

                throw new ValidationException($"Failed to change password: {errors}");
            }

            await RevokeAllActiveTokensAsync(user.Id, null);

            _Logger.LogInformation("Password changed for user {UserId}.", user.Id);

            return true;
        }

        private async Task<ResponseUserDto> IssueTokensAsync(ApplicationUser user,
                                                             string? ipAddress,
                                                             string? refreshTokenValue = null,
                                                             string? refreshTokenHash = null)
        {
            var roles = (await _UserManager.GetRolesAsync(user)).ToList();

            Guid? roleUserId = roles.Any(role => RoleNames.All.Contains(role))
                ? Guid.Parse(user.Id)
                : null;

            var (accessToken, expiration) = BuildAccessToken(user, roles, roleUserId);

            var refreshTokenDays = _Configuration.GetValue("Jwt:RefreshTokenDays", 7);
            var tokenValue = refreshTokenValue ?? GenerateRefreshTokenValue();
            var tokenHash = refreshTokenHash ?? HashToken(tokenValue);

            var refreshToken = new RefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = tokenHash,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(refreshTokenDays),
                CreatedByIp = ipAddress
            };

            await _UnitOfWork.RefreshToken.AddAsync(refreshToken);
            await _UnitOfWork.SaveChangesAsync();

            return new ResponseUserDto
            {
                Token = accessToken,
                Expiration = expiration,
                RefreshToken = tokenValue,
                RefreshTokenExpiration = refreshToken.Expires,
                Email = user.Email,
                Roles = roles,
                RoleUserId = roleUserId,
                EmailConfirmed = user.EmailConfirmed,
                ConfirmationEmailSent = true
            };
        }

        private (string Token, DateTime Expiration) BuildAccessToken(ApplicationUser user,
                                                                     List<string> roles,
                                                                     Guid? roleUserId)
        {
            var jwtKey = _Configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT key is not configured.");

            if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
            {
                throw new InvalidOperationException("The JWT key must be at least 32 bytes.");
            }

            var jwtIssuer = _Configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("JWT issuer is not configured.");

            var jwtAudience = _Configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("JWT audience is not configured.");

            var accessTokenMinutes = _Configuration.GetValue("Jwt:AccessTokenMinutes", 15);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("extension_userId", user.Id),
                new Claim("extension_FullName", user.FullName ?? string.Empty)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            if (roleUserId.HasValue)
            {
                claims.Add(new Claim("roleUserId", roleUserId.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(accessTokenMinutes),
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), token.ValidTo);
        }

        private async Task RevokeAllActiveTokensAsync(string userId, string? ipAddress)
        {
            var activeTokens = await _UnitOfWork.RefreshToken.GetActiveByUserIdAsync(userId);

            foreach (var token in activeTokens)
            {
                token.Revoked = DateTime.UtcNow;
                token.RevokedByIp = ipAddress;

                _UnitOfWork.RefreshToken.Update(token);
            }

            await _UnitOfWork.SaveChangesAsync();
        }

        private static string DecodeToken(string encodedToken)
        {
            try
            {
                return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedToken));
            }
            catch (FormatException)
            {
                throw new ValidationException("The token is not valid.");
            }
            catch (ArgumentException)
            {
                throw new ValidationException("The token is not valid.");
            }
        }

        private static string GenerateRefreshTokenValue()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }

        private static string HashToken(string token)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        }
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartCore.UnitOfWork;
using SmartDto;
using SmartInterfaces.IService;
using SmartModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
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

        public UserService(IUnitOfWork unitOfWork,
                           IMapper mapper,
                           IConfiguration configuration,
                           UserManager<ApplicationUser> userManager,
                           RoleManager<IdentityRole> roleManager)
        {
            _UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _RoleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<ResponseUserDto> AuthenticateUserAsync(UserLoginDto dto)
        {
            var userExists = await _UnitOfWork.User.GetUserByEmailAsync(dto.Email!);

            if (userExists == null)
            {
                throw new AuthenticationException("Invalid email or password.");
            }

            if (!userExists.EmailConfirmed)
            {
                throw new AuthenticationException("Email not confirmed. Please confirm your email before logging in.");
            }

            var passwordHasher = new PasswordHasher<ApplicationUser>();

            var results = passwordHasher.VerifyHashedPassword(userExists, userExists.Password!, dto.Password!);

            if (results == PasswordVerificationResult.Failed)
            {
                userExists.AccessFailedCount++;

                if (userExists.AccessFailedCount >= 5)
                {
                    userExists.LockoutEnd = DateTimeOffset.Now.AddMinutes(5);

                    userExists.LockoutEnabled = true;

                    throw new AuthenticationException("Your account is blocked due to multiple failed login attempts. Please try again after 15 minutes. 😔");
                }

                _UnitOfWork.User.Update(userExists);

                await _UnitOfWork.SaveChangesAsyc();
            }
            else
            {
                userExists.AccessFailedCount = 0;

                _UnitOfWork.User.Update(userExists);

                await _UnitOfWork.SaveChangesAsyc();   
                
                return await GenerateTokenAsycAsync(_Mapper.Map<UserDto>(userExists));
            }

            throw new AuthenticationException("Invalid email or password.");
        }

        private async Task<ResponseUserDto> GenerateTokenAsycAsync(UserDto userDto)
        {
            var jwtKey = _Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured.");
            var jwtIssuer = _Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT issuer is not configured.");
            var jwtAudience = _Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT audience is not configured.");

            var roles = userDto.Roles ?? new List<string>();

            if (!roles.Any() &&
                !string.IsNullOrWhiteSpace(userDto.Email))
            {
                var appUser = await _UnitOfWork.User.GetUserByEmailAsync(userDto.Email);

                if (appUser != null)
                {
                    var fetchRoles = await _UserManager.GetRolesAsync(appUser);
                    roles = fetchRoles.ToList();
                }
            }

            Guid? roleUserId = null;

            if (!string.IsNullOrWhiteSpace(userDto.Email))
            {
                if (roles.Contains("Supplier"))
                {
                    var supplier = await _UnitOfWork.Supplier.GetSupplierByEmail(userDto.Email);
                    if (supplier != null)
                    {
                        roleUserId = supplier.SupplierId;
                    }

                }
                else if (roles.Contains("SpazaOwner"))
                {
                    var shopOwner = await _UnitOfWork.ShopOwner.GetShopOwnerByEmail(userDto.Email);
                    if (shopOwner != null)
                    {
                        roleUserId = shopOwner.ShopOwnerId;
                    }
                }
                else
                {
                    var driver = await _UnitOfWork.Driver.GetDriverByEmail(userDto.Email);
                    if (driver != null)
                    {
                        roleUserId = driver.DriverId;
                    }
                }
            }

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userDto.Email!),
                new Claim(JwtRegisteredClaimNames.Email, userDto.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                new Claim("extension_userId", userDto.Id!),
                new Claim("extension_FullName", userDto.FullName ?? string.Empty)
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
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: cred
            );

            return new ResponseUserDto
            {
                Expiration = token.ValidTo,
                Email = userDto.Email,
                Roles = roles,
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };
        }

        public Task<bool> ChangePasswordAsync(PasswordChangeDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseUserDto> CreateUserAsync(UserDto dto)
        {
            var existingUser = await _UnitOfWork.User.GetUserByEmailAsync(dto.Email!);

            if (existingUser != null)
            {
                throw new Exception("A user with this email already exists.");
            }

            var passwordHasher = new PasswordHasher<ApplicationUser>();

            var user = _Mapper.Map<ApplicationUser>(dto);

            user.Password = passwordHasher.HashPassword(user, dto.Password!);

            user.Id = Guid.NewGuid().ToString();
            user.CreationTime = DateTime.Now;
            user.UserName = dto.Email;
            user.PhoneNumber = dto.WhatsAppNumber;

            var role = await _UnitOfWork.User.GetUserRole(dto.Role ?? "User");

            if (role == null)
            {
                throw new Exception("Specified role does not exist.");
            }

            if (!await _RoleManager.RoleExistsAsync(role))
            {
                var newRole = new IdentityRole(role);

                await _RoleManager.CreateAsync(newRole);
            }

            var result = await _UserManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));

                throw new Exception($"Failed to create user: {errors}");
            }

            var token = await _UserManager.GenerateEmailConfirmationTokenAsync(user);

            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var encodedUserId = Uri.EscapeDataString(user.Id);

            //var callbackUrl = $"{_Configuration["AppSettings:ClientUrl"]}/confirm-email?userId={encodedUserId}&token={encodedToken}";

            //var emailData = new EmailSenderDto
            //{
            //    callbackUrl = callBackUrl,
            //    Email = user.Email,
            //    Role = dto.Role,
            //    FullName = user.FullName,
            //    Password = dto.Password
            //}

            if (role == "Supplier")
            {
                var supplier = _Mapper.Map<Supplier>(dto);
                supplier.SupplierId = Guid.Parse(user.Id);
                supplier.CreatedAt = user.CreationTime;
                await _UnitOfWork.Supplier.AddAsync(supplier);
                await _UnitOfWork.SaveChangesAsyc();

                var userWithToken = await UserRole(user, role);

                return userWithToken;
            }
            else if (role == "SpazaOwner")
            {
                var shopOwner = _Mapper.Map<ShopsOwner>(dto);
                shopOwner.ShopOwnerId = Guid.Parse(user.Id);
                shopOwner.CreatedAt = user.CreationTime;
                await _UnitOfWork.ShopOwner.AddAsync(shopOwner);
                await _UnitOfWork.SaveChangesAsyc();

                var userWithToken = await UserRole(user, role);

                return userWithToken;
            }
            else if (role == "Driver")
            {
                var driver = _Mapper.Map<Drivers>(dto);
                driver.DriverId = Guid.Parse(user.Id);
                driver.CreationTime = user.CreationTime;
                await _UnitOfWork.Driver.AddAsync(driver);
                await _UnitOfWork.SaveChangesAsyc();

                var userWithToken = await UserRole(user, role);

                return userWithToken;
            }
            else
            {
                throw new Exception("User created successfully, but email sending is not implemented in this example. Please implement email sending to complete the registration process.");
            }
        }

        private async Task<ResponseUserDto> UserRole(ApplicationUser user, string role)
        {
            var isInRole = await _UserManager.IsInRoleAsync(user, role);

            if (!isInRole)
            {
                var addToRoleResult = await _UserManager.AddToRoleAsync(user, role);

                if (!addToRoleResult.Succeeded)
                {
                    await _UserManager.DeleteAsync(user);

                    var errors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to assign role to user: {errors}");
                }
            }

            var userRoles = await _UserManager.GetRolesAsync(user);
            var userDto = _Mapper.Map<UserDto>(user);
            userDto.Roles = userRoles.ToList();

            var userWithToken = await GenerateTokenAsycAsync(userDto);

            return userWithToken;
        }

        public Task<bool> EmailConfirmationAsync(string userId, string token)
        {
            throw new NotImplementedException();
        }

        public Task<string> GeneratePasswordResetTOkenRepositoryAsnc(string email)
        {
            throw new NotImplementedException();
        }

        public Task<UserDto> UpdateUserAsync(string userId, UserDto dto, string token = null!)
        {
            throw new NotImplementedException();
        }
    }
}

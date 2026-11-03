using System.Security.Claims;
using SmartConfiguration;

namespace SmartSpazaAPI.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? principal.FindFirstValue("extension_userId")
                   ?? principal.FindFirstValue("sub");
        }

        public static string RequireUserId(this ClaimsPrincipal principal)
        {
            return principal.GetUserId()
                   ?? throw new UnauthorizedException("The authenticated user could not be identified.");
        }

        public static Guid? GetRoleUserId(this ClaimsPrincipal principal)
        {
            var value = principal.FindFirstValue("roleUserId");

            return Guid.TryParse(value, out var id) ? id : null;
        }

        public static Guid RequireRoleUserId(this ClaimsPrincipal principal)
        {
            return principal.GetRoleUserId()
                   ?? throw new ForbiddenException("Your account is not associated with a profile that can access this resource.");
        }
    }
}

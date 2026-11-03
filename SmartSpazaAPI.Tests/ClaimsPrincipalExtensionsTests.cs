using System.Security.Claims;
using SmartConfiguration;
using SmartSpazaAPI.Extensions;
using Xunit;

namespace SmartSpazaAPI.Tests
{
    public class ClaimsPrincipalExtensionsTests
    {
        [Fact]
        public void RequireUserId_ReturnsNameIdentifier()
        {
            var principal = CreatePrincipal(userId: "user-1", roleUserId: null);

            Assert.Equal("user-1", principal.RequireUserId());
        }

        [Fact]
        public void RequireUserId_FallsBackToExtensionClaim()
        {
            var identity = new ClaimsIdentity(new[] { new Claim("extension_userId", "user-2") }, "test");
            var principal = new ClaimsPrincipal(identity);

            Assert.Equal("user-2", principal.RequireUserId());
        }

        [Fact]
        public void RequireUserId_ThrowsWhenMissing()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity());

            Assert.Throws<UnauthorizedException>(() => principal.RequireUserId());
        }

        [Fact]
        public void RequireRoleUserId_ReturnsParsedProfileId()
        {
            var profileId = Guid.NewGuid();
            var principal = CreatePrincipal(userId: "user-1", roleUserId: profileId);

            Assert.Equal(profileId, principal.RequireRoleUserId());
        }

        [Fact]
        public void RequireRoleUserId_ThrowsWhenMissing()
        {
            var principal = CreatePrincipal(userId: "user-1", roleUserId: null);

            Assert.Throws<ForbiddenException>(() => principal.RequireRoleUserId());
        }

        private static ClaimsPrincipal CreatePrincipal(string userId, Guid? roleUserId)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId)
            };

            if (roleUserId.HasValue)
            {
                claims.Add(new Claim("roleUserId", roleUserId.Value.ToString()));
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        }
    }
}
